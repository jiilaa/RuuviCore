using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Grains.DataParsers;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using Serilog;

namespace net.jommy.RuuviCore.Grains
{
    [ImplicitStreamSubscription("MeasurementStream")]
    public class RuuviTagGrain : Grain, IRuuviTag, IAsyncObserver<MeasurementEnvelope>
    {
        private readonly IPersistentState<RuuviTagState> _ruuviTagState;
        private const string GlobalDeviceEndpoint = "global.azure-devices-provisioning.net";
        private DeviceClient _azureClient;
        private DateTime _lastPushTime = DateTime.MinValue;

        public RuuviTagGrain(
            [PersistentState(nameof(RuuviTagState), "RuuviStorage")]
            IPersistentState<RuuviTagState> ruuviTagState)
        {
            _ruuviTagState = ruuviTagState;
        }

        public override async Task OnActivateAsync()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            if (_ruuviTagState.State.UseAzure)
            {
                _azureClient = await ConnectToAzureIoT();
                if (_azureClient == null)
                {
                    Log.Error("Could not initialize the connection to Azure IoT. Disabling the connection.");
                    _ruuviTagState.State.UseAzure = false;
                }
            }

            var streamProvider = GetStreamProvider(RuuviCoreConstants.StreamProviderName);
            var stream = streamProvider.GetStream<MeasurementEnvelope>(this.GetPrimaryKey(), "MeasurementStream");
            await stream.SubscribeAsync(this);
        }

        public async Task Initialize(string macAddress, string name, DataSavingOptions dataSavingOptions)
        {
            if (macAddress.ToActorGuid() != this.GetPrimaryKey())
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
            return Task.FromResult(new DataSavingOptions
            {
                CalculateAverages = _ruuviTagState.State.CalculateAverages,
                DataSavingInterval = _ruuviTagState.State.DataSavingInterval,
                StoreAcceleration = _ruuviTagState.State.StoreAcceleration
            });
        }

        public async Task SetDataSavingOptions(DataSavingOptions options)
        {
            _ruuviTagState.State.DataSavingInterval = options.DataSavingInterval;
            _ruuviTagState.State.CalculateAverages = options.CalculateAverages;
            _ruuviTagState.State.StoreAcceleration = options.StoreAcceleration;
            await _ruuviTagState.WriteStateAsync();
        }

        public async Task StoreMeasurementData(Measurements measurements)
        {
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
                AddMeasurements(measurements);
            }
            if (TimeSpan.FromSeconds(_ruuviTagState.State.DataSavingInterval) <= (measurements.Timestamp - _lastPushTime))
            {
                var pushedSuccessfully = await PushData(measurements);
                if (pushedSuccessfully)
                {
                    Log.Debug("{Identity}: Measurements sent successfully.", GetIdentity());
                    if (_ruuviTagState.State.CalculateAverages)
                    {
                        _ruuviTagState.State.LatestMeasurements.Clear();
                    }
                    _lastPushTime = measurements.Timestamp;
                }
                else
                {
                    Log.Warning("{Identity}: There was a problem sending the measurements.", GetIdentity());
                }
            }
        }

        private async Task StoreMeasurementData(DateTime timeStamp, short signalStrength, byte[] data)
        {
            var valid = TryParseMeasurements(data, _ruuviTagState.State.DiscardMinMaxValues, out var measurements);
            if (!valid)
            {
                Log.Error("{Identity}: Discarding packet data with invalid values.", GetIdentity());
                return;
            }
            measurements.Timestamp = timeStamp;
            measurements.RSSI = signalStrength;

            await StoreMeasurementData(measurements);
        }

        private async Task<bool> PushData(Measurements measurements)
        {
            // TODO: Implement logic to push old data too if influxDB hasn't been reachable during the last attempt.
            measurements = _ruuviTagState.State.CalculateAverages ? CalculateAverageMeasurements() : measurements;

            Log.Information("{Identity}: Pushing measurements to InfluxBridge: {measurements}", GetIdentity(), measurements);

            var result = await GrainFactory.GetGrain<IInfluxBridge>(0)
                .WriteMeasurements(_ruuviTagState.State.MacAddress, _ruuviTagState.State.StoreName ? _ruuviTagState.State.Name : null, measurements);

            if (_ruuviTagState.State.UseAzure)
            {
                await SendTelemetry(measurements);
            }

            return result;
        }

        private Measurements CalculateAverageMeasurements()
        {
            Acceleration accelerationAverage = null;

            var averageMeasurements = new Measurements
            {
                // No point calculating battery level average, just use the latest
                BatteryVoltage = _ruuviTagState.State.LatestMeasurements.Last().BatteryVoltage,
                Humidity = _ruuviTagState.State.LatestMeasurements.Select(m => m.Humidity).Average(),
                Pressure = _ruuviTagState.State.LatestMeasurements.Select(m => m.Pressure).Average(),
                Temperature = _ruuviTagState.State.LatestMeasurements.Select(m => m.Temperature).Average(),
                Timestamp = _ruuviTagState.State.LatestMeasurements.Last().Timestamp
            };

            if (_ruuviTagState.State.StoreAcceleration)
            {
                accelerationAverage = new Acceleration
                {
                    XAxis = _ruuviTagState.State.LatestMeasurements.Select(m => m.Acceleration.XAxis).Average(),
                    YAxis = _ruuviTagState.State.LatestMeasurements.Select(m => m.Acceleration.YAxis).Average(),
                    ZAxis = _ruuviTagState.State.LatestMeasurements.Select(m => m.Acceleration.ZAxis).Average()
                };
            }

            averageMeasurements.Acceleration = accelerationAverage;

            return averageMeasurements;
        }

        public Task<List<Measurements>> GetCachedMeasurements()
        {
            return Task.FromResult(_ruuviTagState.State.LatestMeasurements);
        }

        public async Task UseAzure(AzureState state, string scopeId, string primaryKey)
        {
            switch (state)
            {
                case AzureState.On:
                    _ruuviTagState.State.UseAzure = true;
                    // Use existing values if not given
                    _ruuviTagState.State.AzureScopeId = scopeId ?? _ruuviTagState.State.AzureScopeId;
                    _ruuviTagState.State.AzurePrimaryKey = primaryKey ?? _ruuviTagState.State.AzurePrimaryKey;
                    break;
                case AzureState.Off:
                    _ruuviTagState.State.UseAzure = false;
                    // Clear the primary key and scope ID values
                    _ruuviTagState.State.AzurePrimaryKey = null;
                    _ruuviTagState.State.AzureScopeId = null;
                    break;
                case AzureState.Paused:
                    _ruuviTagState.State.UseAzure = false;
                    // Keep the primary key and scope ID values
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(state), (int) state, typeof(AzureState) );
            }

            await _ruuviTagState.WriteStateAsync();
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

        private void AddMeasurements(Measurements measurements)
        {
            _ruuviTagState.State.LatestMeasurements ??= new List<Measurements>();
            _ruuviTagState.State.LatestMeasurements.Add(measurements);
        }

        private static bool TryParseMeasurements(byte[] data, bool discardInvalidValues, out Measurements measurements)
        {
            try
            {
                return DataParserFactory.GetParser(data).TryParseMeasurements(data, discardInvalidValues, out measurements);
            }
            catch (Exception e)
            {
                Log.Error("Failed to parse measurements: {errorMessage}", e.Message);
                measurements = null;
                return false;
            }
        }

        private string GetIdentity()
        {
            return _ruuviTagState.State.Name ?? _ruuviTagState.State.MacAddress;
        }

        #region Azure IoT

        private async Task<DeviceClient> ConnectToAzureIoT()
        {
            Log.Information("Connecting {identity} to Azure IoT.", GetIdentity());
            using var security =
                new SecurityProviderSymmetricKey(this.GetPrimaryKeyString(), _ruuviTagState.State.AzurePrimaryKey, null);
            var result = await RegisterDevice(security);
            if (result.Status != ProvisioningRegistrationStatusType.Assigned)
            {
                Log.Error("Failed to register device {identity} to Azure IoT.", GetIdentity());
                return null;
            }

            IAuthenticationMethod auth =
                new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId, security.GetPrimaryKey());
            return DeviceClient.Create(result.AssignedHub, auth, TransportType.Mqtt);
        }

        private async Task<DeviceRegistrationResult> RegisterDevice(SecurityProviderSymmetricKey security)
        {
            Log.Information("Registering device {identity}.", GetIdentity());

            using var transport = new ProvisioningTransportHandlerMqtt(TransportFallbackType.TcpOnly);
            var provClient = ProvisioningDeviceClient.Create(GlobalDeviceEndpoint, _ruuviTagState.State.AzureScopeId, security, transport);

            var result = await provClient.RegisterAsync();

            Log.Information("Registration result: {result}.", result.Status);
            return result;
        }

        private async Task SendTelemetry(Measurements measurements)
        {
            if (_azureClient == null)
            {
                _azureClient = await ConnectToAzureIoT();
                if (_azureClient == null)
                {
                    Log.Error("Could not initialize the connection to Azure IoT. Disabling the connection.");
                    _ruuviTagState.State.UseAzure = false;
                }
            }
            Log.Information("{Identity}: Sending measurements to Azure IoT.", GetIdentity());

            var telemetryDataPoint = new
            {
                humidity = measurements.Humidity,
                pressure = measurements.Pressure,
                temp = measurements.Temperature
            };
            var messageString = JsonSerializer.Serialize(telemetryDataPoint);
            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            await _azureClient.SendEventAsync(message);
        }

        #endregion // Azure IoT

        public async Task OnNextAsync(MeasurementEnvelope measurementEnvelope, StreamSequenceToken token = null)
        {
            if (!_ruuviTagState.State.Initialized)
            {
                Log.Warning("An uninitialized RuuviTag {macAddress} receiving data with signal strength {signalStrength}. (Ignoring packet)", 
                    measurementEnvelope.MacAddress,
                    measurementEnvelope.SignalStrength);
                return;
            }
            await StoreMeasurementData(measurementEnvelope.Timestamp, measurementEnvelope.SignalStrength, measurementEnvelope.Data);
            await GrainFactory.GetGrain<IRuuviTagRegistry>(0).Refresh(_ruuviTagState.State.MacAddress, measurementEnvelope.Timestamp);
        }

        public Task OnCompletedAsync()
        {
            return Task.CompletedTask;
        }

        public Task OnErrorAsync(Exception ex)
        {
            Log.Error(ex, "{ruuviTag} | Stream error.", _ruuviTagState.State.Name ?? _ruuviTagState.State.MacAddress);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<RuuviTag> GetTag()
        {
            return Task.FromResult(new RuuviTag
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
                if (ruuviTag.MacAddress.ToActorGuid() != this.GetPrimaryKey())
                {
                    throw new ArgumentException("MAC address used to fetch RuuviTag does not match the MAC address passed as parameter.");
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

            if (ruuviTag.AlertRules.Count != _ruuviTagState.State.AlertRules.Count ||
                !ruuviTag.AlertRules.Any(AlertRuleDiffers))
            {
                dirty = true;
                _ruuviTagState.State.AlertRules = ruuviTag.AlertRules;
            }
            
            if (dirty)
            {
                await _ruuviTagState.WriteStateAsync();
                if (updateRegistry)
                {
                    await GrainFactory.GetGrain<IRuuviTagRegistry>(0).AddOrUpdate(_ruuviTagState.State.MacAddress, _ruuviTagState.State.Name);
                }
            }
        }

        private bool AlertRuleDiffers(KeyValuePair<string, AlertThresholds> newRules)
        {
            if (_ruuviTagState.State.AlertRules.TryGetValue(newRules.Key, out var value))
            {
                return value.MinValidValue != newRules.Value.MinValidValue || value.MaxValidValue != newRules.Value.MaxValidValue;
            }

            return true;
        }
    }
}
