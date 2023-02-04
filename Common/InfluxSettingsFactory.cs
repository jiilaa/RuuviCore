using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace net.jommy.RuuviCore.Common;

public class InfluxSettingsFactory : IInfluxSettingsFactory
{
    private readonly ILogger<InfluxSettingsFactory> _logger;
    private readonly InfluxBridgeList _influxBridgeList;

    public InfluxSettingsFactory(ILogger<InfluxSettingsFactory> logger, IOptions<InfluxBridgeList> influxBridgeList)
    {
        _logger = logger;
        _influxBridgeList = influxBridgeList.Value;
    }

    public InfluxSettings GetSettings(string name)
    {
        var settings = _influxBridgeList.Bridges.FirstOrDefault(s => s.BridgeName == name);

        if (settings == null)
        {
            _logger.LogWarning("Could not find settings for influx bridge named {bridgeName}. App configuration in old format? Continuing with default values.", name);
        }

        return settings ?? new InfluxSettings();
    }
}