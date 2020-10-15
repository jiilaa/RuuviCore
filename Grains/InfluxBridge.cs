using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Collector;
using InfluxDB.Collector.Diagnostics;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Concurrency;
using Serilog;

namespace net.jommy.RuuviCore.Grains
{
    [StatelessWorker]
    public class InfluxBridge : Grain, IInfluxBridge
    {
        private MetricsCollector _metricsCollector;

        public InfluxBridge()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public override Task OnActivateAsync()
        {
            _metricsCollector = new CollectorConfiguration()
                .Batch.AtInterval(TimeSpan.FromSeconds(2))
                .WriteTo.InfluxDB("http://localhost:8086", "ruuvidata")
                .CreateCollector();
            CollectorLog.RegisterErrorHandler((message, exception) =>
            {
                Log.Error(exception, message);
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
                _metricsCollector.Write("measurements", new Dictionary<string, object>
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
                Log.Error(e, "Error saving data to InfluxDB: {errorMessage}.", e.Message);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
