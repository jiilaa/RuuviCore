using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Grains.DataParsers;
using net.jommy.RuuviCore.Interfaces;

using Orleans;
using Orleans.Runtime;

namespace net.jommy.RuuviCore.Grains;

[GrainType(Constants.GrainTypeRuuviTag)]
public class RuuviTagGrain : Grain, IRuuviTag
{
    private readonly IPersistentState<RuuviTagState> _ruuviTagState;
    private readonly ILogger<RuuviTagGrain> _logger;
    private DateTime _lastPushTime = DateTime.MinValue;

    public RuuviTagGrain(
        [PersistentState(nameof(RuuviTagState), RuuviCoreConstants.GrainStorageName)]
        IPersistentState<RuuviTagState> ruuviTagState,
        ILogger<RuuviTagGrain> logger)
    {
        _ruuviTagState = ruuviTagState;
        _logger = logger;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await GrainFactory.GetGrain<IRuuviTagRegistry>(0)
            .AddOrUpdate(this.GetPrimaryKeyString(), _ruuviTagState.State.Name);
    }

    public override async Task OnDeactivateAsync(DeactivationReason reason, CancellationToken cancellationToken)
    {
        // Lazy attempt to save the latest signal strength, doesn't matter too much if it fails.
        // Everything else is saved when data is changed
        await _ruuviTagState.WriteStateAsync(cancellationToken);
    }

    public async Task Initialize(string macAddress, string name, DataSavingOptions dataSavingOptions)
    {
        if (macAddress != this.GetPrimaryKeyString())
        {
            throw new ArgumentException("MAC address does not match the actor primary key.");
        }

        _ruuviTagState.State.MacAddress = macAddress;
        _ruuviTagState.State.Name = name;
        _ruuviTagState.State.DataSavingInterval = dataSavingOptions.DataSavingInterval;
        _ruuviTagState.State.CalculateAverages = dataSavingOptions.CalculateAverages;
        _ruuviTagState.State.StoreAcceleration = dataSavingOptions.StoreAcceleration;
        _ruuviTagState.State.DiscardMinMaxValues = dataSavingOptions.DiscardMinMaxValues;
        _ruuviTagState.State.Initialized = true;
        await _ruuviTagState.WriteStateAsync();
        await GrainFactory.GetGrain<IRuuviTagRegistry>(0).AddOrUpdate(macAddress, name);
    }

    public async Task SetName(string name)
    {
        _ruuviTagState.State.Name = name;
        await _ruuviTagState.WriteStateAsync();
        await GrainFactory.GetGrain<IRuuviTagRegistry>(0).AddOrUpdate(_ruuviTagState.State.MacAddress, name);
    }

    public Task<string> GetName()
    {
        return Task.FromResult(_ruuviTagState.State.Name);
    }

    public Task<DataSavingOptions> GetDataSavingOptions()
    {
        return Task.FromResult(
            new DataSavingOptions
            {
                CalculateAverages = _ruuviTagState.State.CalculateAverages,
                DataSavingInterval = _ruuviTagState.State.DataSavingInterval,
                StoreAcceleration = _ruuviTagState.State.StoreAcceleration
            });
    }

    public async Task SetDataSavingOptions(DataSavingOptions options)
    {
        if (options.BucketSize.Minutes >= 60)
        {
            if (options.BucketSize.Minutes % 60 != 0)
            {
                throw new ArgumentException("Bucket size must be in full hours if greater than 60 minutes.");
            }
        }
        else if (60 % options.BucketSize.Minutes != 0)
        {
            throw new ArgumentException("Bucket size must divide evenly into 60 minutes if less than 60 minutes.");
        }

        _ruuviTagState.State.DataSavingInterval = options.DataSavingInterval;
        _ruuviTagState.State.CalculateAverages = options.CalculateAverages;
        _ruuviTagState.State.StoreAcceleration = options.StoreAcceleration;
        _ruuviTagState.State.BucketSize = options.BucketSize;
        await _ruuviTagState.WriteStateAsync();
    }

    public async Task StoreMeasurementData(MeasurementDTO measurements)
    {
        _logger.LogDebug(
            "{Identity}: Received measurements with timestamp {timestamp} and sequence number {sequenceNumber}",
            GetIdentity(),
            measurements.Timestamp,
            measurements.SequenceNumber);

        // We are no longer interested if older values have already been pushed.
        if (measurements.Timestamp < _lastPushTime)
        {
            return;
        }

        // Gateway reports the acceleration values, so drop them if saving is not wanted
        if (!_ruuviTagState.State.StoreAcceleration)
        {
            measurements.Acceleration = null;
        }

        if (_ruuviTagState.State.CalculateAverages)
        {
            var bucketFull = AddMeasurementsToBucket(measurements);

            if (bucketFull)
            {
                foreach (var bucketAverage in _ruuviTagState.State.CompletedBucketAverages)
                {
                    var pushedSuccessfully = await PushDataAsync(bucketAverage.Measurement);
                    if (!pushedSuccessfully)
                    {
                        _logger.LogWarning(
                            "Failed to push bucket average data for {Identity}. Storing bucket average for later retry.",
                            GetIdentity());
                    }
                    else
                    {
                        bucketAverage.Sent = true;
                    }
                }

                // Remove sent averages
                _ruuviTagState.State.CompletedBucketAverages.RemoveAll(b => b.Sent);
            }
        }
        else if (TimeSpan.FromSeconds(_ruuviTagState.State.DataSavingInterval)
                 <= measurements.Timestamp - _lastPushTime)
        {
            var pushedSuccessfully = await PushDataAsync(measurements);
            if (pushedSuccessfully)
            {
                _lastPushTime = measurements.Timestamp;
            }
            else
            {
                _logger.LogWarning("{Identity}: There was a problem sending the measurements.", GetIdentity());
            }
        }
    }

    private async Task StoreMeasurementDataAsync(DateTime timeStamp, short signalStrength, byte[] data)
    {
        var valid = TryParseMeasurements(data, _ruuviTagState.State.DiscardMinMaxValues, out var measurements);
        if (!valid)
        {
            _logger.LogError("{Identity}: Discarding packet data with invalid values.", GetIdentity());
            return;
        }

        measurements.Timestamp = timeStamp;
        measurements.RSSI = signalStrength;

        await StoreMeasurementData(measurements);
    }

    private async Task<bool> PushDataAsync(MeasurementDTO measurements)
    {
        // TODO: Implement logic to push old data too if influxDB hasn't been reachable during the last attempt.
        _logger.LogInformation("{Identity}: Saving measurements: {measurements}", GetIdentity(), measurements);

        var result = await GrainFactory.GetGrain<IInfluxBridge>(0)
            .WriteMeasurements(
                _ruuviTagState.State.MacAddress,
                _ruuviTagState.State.StoreName ? _ruuviTagState.State.Name : null,
                measurements);

        return result;
    }

    public Task<List<MeasurementDTO>> GetCachedMeasurements()
    {
        return Task.FromResult(_ruuviTagState.State.CurrentBucketMeasurements);
    }

    public Task<bool> MeasurementsAllowedThroughGateway()
    {
        return Task.FromResult(_ruuviTagState.State.AllowMeasurementsThroughGateway);
    }

    public async Task AllowMeasurementsThroughGateway(bool allowed)
    {
        var existingValue = _ruuviTagState.State.AllowMeasurementsThroughGateway;
        _ruuviTagState.State.AllowMeasurementsThroughGateway = allowed;

        // Raspberry Pi optimization, no unnecessary writes to SD card.
        if (allowed != existingValue)
        {
            await _ruuviTagState.WriteStateAsync();
        }
    }

    private bool AddMeasurementsToBucket(MeasurementDTO measurements)
    {
        var bucketFull = false;
        var bucketSize = _ruuviTagState.State.BucketSize;

        // Initialize first bucket
        if (_ruuviTagState.State.CurrentBucketStartTime == null)
        {
            _ruuviTagState.State.CurrentBucketStartTime = GetBucketStartTime(measurements.Timestamp, bucketSize);
            _ruuviTagState.State.CurrentBucketMeasurements = [];
        }

        if (measurements.Timestamp < _ruuviTagState.State.CurrentBucketStartTime)
        {
            // Old measurement, ignore
            return false;
        }

        var currentBucketEnd = _ruuviTagState.State.CurrentBucketStartTime.Value + bucketSize;

        // Check if measurement belongs to current bucket
        if (measurements.Timestamp >= currentBucketEnd)
        {
            // Bucket expired - calculate average and store it
            if (_ruuviTagState.State.CurrentBucketMeasurements.Count > 0)
            {
                var bucketAverage = CalculateAverageMeasurements(
                    _ruuviTagState.State.CurrentBucketMeasurements,
                    currentBucketEnd); // Use bucket end time as timestamp

                _ruuviTagState.State.CompletedBucketAverages ??= [];
                _ruuviTagState.State.CompletedBucketAverages.Add(
                    new CachedMeasurement { Sent = false, Measurement = bucketAverage });
                bucketFull = true;
            }

            // Start new bucket
            _ruuviTagState.State.CurrentBucketStartTime = GetBucketStartTime(measurements.Timestamp, bucketSize);
            _ruuviTagState.State.CurrentBucketMeasurements = [];
        }

        // Add measurement to current bucket
        _ruuviTagState.State.CurrentBucketMeasurements.Add(measurements);

        return bucketFull;
    }

    // Align bucket start times to fixed intervals
    private static DateTime GetBucketStartTime(DateTime timestamp, TimeSpan bucketSize)
    {
        var ticks = timestamp.Ticks / bucketSize.Ticks;
        return new DateTime(ticks * bucketSize.Ticks, timestamp.Kind);
    }

    private static MeasurementDTO CalculateAverageMeasurements(List<MeasurementDTO> measurements, DateTime timestamp)
    {
        if (measurements == null || measurements.Count == 0)
        {
            return null;
        }

        var averageMeasurements = new MeasurementDTO
        {
            BatteryVoltage = measurements.Last().BatteryVoltage,
            Humidity = measurements.Select(m => m.Humidity).Average(),
            Pressure = measurements.Select(m => m.Pressure).Average(),
            Temperature = measurements.Select(m => m.Temperature).Average(),
            Timestamp = timestamp
        };

        return averageMeasurements;
    }

    private bool TryParseMeasurements(byte[] data, bool discardInvalidValues, out MeasurementDTO measurements)
    {
        try
        {
            return DataParserFactory.GetParser(data).TryParseMeasurements(data, discardInvalidValues, out measurements);
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to parse measurements: {errorMessage}", e.Message);
            measurements = null;
            return false;
        }
    }

    private string GetIdentity()
    {
        return _ruuviTagState.State.Name ?? _ruuviTagState.State.MacAddress;
    }

    public async Task ReceiveMeasurements(MeasurementEnvelope measurementEnvelope)
    {
        if (!_ruuviTagState.State.Initialized)
        {
            _logger.LogWarning(
                "An uninitialized RuuviTag {macAddress} receiving data with signal strength {signalStrength}. (Ignoring packet)",
                this.GetPrimaryKeyString(),
                measurementEnvelope.SignalStrength);
            return;
        }

        _ruuviTagState.State.SignalStrength =
            measurementEnvelope.SignalStrength.GetValueOrDefault(_ruuviTagState.State.SignalStrength);

        await StoreMeasurementDataAsync(
            measurementEnvelope.Timestamp,
            _ruuviTagState.State.SignalStrength,
            measurementEnvelope.Data);
        await GrainFactory.GetGrain<IRuuviTagRegistry>(0).Refresh(
            _ruuviTagState.State.MacAddress,
            measurementEnvelope.Timestamp);
    }

    /// <inheritdoc />
    public Task<RuuviTag> GetTag()
    {
        return Task.FromResult(
            new RuuviTag
            {
                Name = _ruuviTagState.State.Name,
                StoreAcceleration = _ruuviTagState.State.StoreAcceleration,
                StoreName = _ruuviTagState.State.StoreName,
                DataSavingInterval = _ruuviTagState.State.DataSavingInterval,
                AllowMeasurementsThroughGateway = _ruuviTagState.State.AllowMeasurementsThroughGateway,
                DiscardMinMaxValues = _ruuviTagState.State.DiscardMinMaxValues
            });
    }

    public async Task Edit(RuuviTag ruuviTag)
    {
        var dirty = false;
        var updateRegistry = ruuviTag.Name != _ruuviTagState.State.Name;

        if (!_ruuviTagState.State.Initialized)
        {
            if (ruuviTag.MacAddress != this.GetPrimaryKeyString())
            {
                throw new ArgumentException(
                    "MAC address used to fetch RuuviTag does not match the MAC address passed as parameter.");
            }

            dirty = true;
            updateRegistry = true;
            _ruuviTagState.State.MacAddress = ruuviTag.MacAddress;
            _ruuviTagState.State.Initialized = true;
        }

        if (ruuviTag.Name != _ruuviTagState.State.Name)
        {
            dirty = true;
            _ruuviTagState.State.Name = ruuviTag.Name;
        }

        if (ruuviTag.StoreAcceleration != _ruuviTagState.State.StoreAcceleration)
        {
            dirty = true;
            _ruuviTagState.State.StoreAcceleration = ruuviTag.StoreAcceleration;
        }

        if (ruuviTag.StoreName != _ruuviTagState.State.StoreName)
        {
            dirty = true;
            _ruuviTagState.State.StoreName = ruuviTag.StoreName;
        }

        if (ruuviTag.DataSavingInterval != _ruuviTagState.State.DataSavingInterval)
        {
            dirty = true;
            _ruuviTagState.State.DataSavingInterval = ruuviTag.DataSavingInterval;
        }

        if (ruuviTag.AllowMeasurementsThroughGateway != _ruuviTagState.State.AllowMeasurementsThroughGateway)
        {
            dirty = true;
            _ruuviTagState.State.AllowMeasurementsThroughGateway = ruuviTag.AllowMeasurementsThroughGateway;
        }

        if (ruuviTag.DiscardMinMaxValues != _ruuviTagState.State.DiscardMinMaxValues)
        {
            dirty = true;
            _ruuviTagState.State.DiscardMinMaxValues = ruuviTag.DiscardMinMaxValues;
        }

        if (ruuviTag.IncludeInDashboard != _ruuviTagState.State.InDashboard)
        {
            dirty = true;
            _ruuviTagState.State.InDashboard = ruuviTag.IncludeInDashboard;
        }

        if (ruuviTag.AlertRules != null && (ruuviTag.AlertRules == null ||
                                            ruuviTag.AlertRules.Count != _ruuviTagState.State.AlertRules.Count ||
                                            !ruuviTag.AlertRules.Any(AlertRuleDiffers)))
        {
            dirty = true;
            _ruuviTagState.State.AlertRules = ruuviTag.AlertRules;
        }

        if (dirty)
        {
            await _ruuviTagState.WriteStateAsync();
            if (updateRegistry)
            {
                await GrainFactory.GetGrain<IRuuviTagRegistry>(0).AddOrUpdate(
                    _ruuviTagState.State.MacAddress,
                    _ruuviTagState.State.Name);
            }
        }
    }

    private bool AlertRuleDiffers(KeyValuePair<string, AlertThresholds> newRules)
    {
        if (_ruuviTagState.State.AlertRules.TryGetValue(newRules.Key, out var value))
        {
            return value.MinValidValue != newRules.Value.MinValidValue
                   || value.MaxValidValue != newRules.Value.MaxValidValue;
        }

        return true;
    }
}
