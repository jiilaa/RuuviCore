using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using InfluxDB.Collector;
using InfluxDB.Collector.Diagnostics;

using Microsoft.Extensions.Logging;

using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;

using Orleans;
using Orleans.Concurrency;

namespace net.jommy.RuuviCore.Grains;

[StatelessWorker]
public class InfluxBridge : Grain, IInfluxBridge
{
    private MetricsCollector _metricsCollector;
    private InfluxSettings _influxSettings;
    private readonly IInfluxSettingsFactory _influxSettingsFactory;
    private readonly ILogger<InfluxBridge> _logger;

    public InfluxBridge(IInfluxSettingsFactory influxSettingsFactory, ILogger<InfluxBridge> logger)
    {
        _influxSettingsFactory = influxSettingsFactory;
        _logger = logger;
    }

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _influxSettings = _influxSettingsFactory.GetSettings(this.GetPrimaryKeyString());
        _logger.LogDebug(
            "Activating InfluxDB bridge to {InfluxAddress}, database: {InfluxDatabase}, measurement table: {InfluxMeasurementTable}, UserName: {UserName}, Password: {Password}.",
            _influxSettings.InfluxAddress,
            _influxSettings.InfluxDatabase,
            _influxSettings.InfluxMeasurementTable,
            _influxSettings.Username,
            MaskPassword(_influxSettings.Password));
        _metricsCollector = new CollectorConfiguration()
            .Batch.AtInterval(TimeSpan.FromSeconds(2))
            .WriteTo.InfluxDB(
                _influxSettings.InfluxAddress,
                _influxSettings.InfluxDatabase,
                _influxSettings.Username,
                _influxSettings.Password)
            .CreateCollector();
        CollectorLog.RegisterErrorHandler((message, exception) =>
        {
            _logger.LogError(
                "{Message}. Error when activating bridge to influx: {ErrorMessage}",
                message,
                exception?.Message);
        });

        return Task.CompletedTask;
    }

    private static string MaskPassword(string influxSettingsPassword)
    {
        if (string.IsNullOrEmpty(influxSettingsPassword))
        {
            return "<empty>";
        }

        return influxSettingsPassword[0] + new string('*', influxSettingsPassword.Length - 1);
    }

    public Task<bool> WriteMeasurements(string macAddress, string name, MeasurementDTO measurements)
    {
        try
        {
            var tags = new Dictionary<string, string> { { "mac", macAddress } };
            if (name != null)
            {
                tags["name"] = name;
            }

            var fields = new Dictionary<string, object>
            {
                { "Temperature", measurements.Temperature },
                { "Humidity", measurements.Humidity },
                { "Pressure", measurements.Pressure },
                { "Rssi", measurements.RSSI },
                { "BatteryVoltage", measurements.BatteryVoltage }
            };

            // Add air quality measurements if available
            if (measurements.AirQuality != null)
            {
                if (measurements.AirQuality.ParticulateMatter25.HasValue)
                {
                    fields["PM25"] = measurements.AirQuality.ParticulateMatter25.Value;
                }
                if (measurements.AirQuality.CO2Concentration.HasValue)
                {
                    fields["CO2"] = measurements.AirQuality.CO2Concentration.Value;
                }
                if (measurements.AirQuality.VolatileOrganicCompoundsIndex.HasValue)
                {
                    fields["VOC"] = measurements.AirQuality.VolatileOrganicCompoundsIndex.Value;
                }
                if (measurements.AirQuality.NitrogenOxidesIndex.HasValue)
                {
                    fields["NOX"] = measurements.AirQuality.NitrogenOxidesIndex.Value;
                }
            }

            // Add luminosity if available
            if (measurements.Luminosity.HasValue)
            {
                fields["Luminosity"] = measurements.Luminosity.Value;
            }

            _metricsCollector.Write(
                _influxSettings.InfluxMeasurementTable,
                fields,
                tags,
                measurements.Timestamp);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving data to InfluxDB: {errorMessage}.", e.Message);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task<bool> IsValid()
    {
        return Task.FromResult(_influxSettings.BridgeName == this.GetPrimaryKeyString());
    }
}
