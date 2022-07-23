using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Collector;
using InfluxDB.Collector.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Concurrency;

namespace net.jommy.RuuviCore.Grains
{
    [StatelessWorker]
    public class InfluxBridge : Grain, IInfluxBridge
    {
        private MetricsCollector _metricsCollector;
        private readonly InfluxSettings _influxSettings;
        private readonly ILogger<InfluxBridge> _logger;

        public InfluxBridge(IOptions<InfluxSettings> influxOptions, ILogger<InfluxBridge> logger)
        {
            _influxSettings = influxOptions.Value;
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _metricsCollector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromSeconds(2))
                .WriteTo.InfluxDB(_influxSettings.InfluxAddress, _influxSettings.InfluxDatabase, _influxSettings.Username, _influxSettings.Password)
                .CreateCollector();
            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                _logger.LogError(exception, "Error when activating bridge to influx: {errorMessage}", message);
            });
            
            return base.OnActivateAsync();
        }

        public Task<bool> WriteMeasurements(string macAddress, string name, Measurements measurements)
        {
            try
            {
                var tags = new Dictionary<string, string> { {"mac", macAddress}};
                if (name != null)
                {
                    tags["name"] = name;
                }
                _metricsCollector.Write(_influxSettings.InfluxMeasurementTable, new Dictionary<string, object>
                {
                    {"Temperature", measurements.Temperature},
                    {"Humidity", measurements.Humidity},
                    {"Pressure", measurements.Pressure},
                    {"Rssi", measurements.RSSI},
                    {"BatteryVoltage", measurements.BatteryVoltage}
                }, tags, measurements.Timestamp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error saving data to InfluxDB: {errorMessage}.", e.Message);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
