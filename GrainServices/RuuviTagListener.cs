using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Bluez.Objects;
using net.jommy.RuuviCore.Interfaces;
using Orleans;

namespace net.jommy.RuuviCore.GrainServices;

public class RuuviTagListener : AbstractDeviceListener
{
    private readonly ILogger<RuuviTagListener> _logger;
    private const ushort RuuviManufacturerDataKey = 1177;
    private readonly string _grainAddress;

    protected override ushort ManufacturerKey => RuuviManufacturerDataKey;

    public RuuviTagListener(Device device, string deviceAddress, IGrainFactory grainFactory, ILogger<RuuviTagListener> logger)
        : base(device, deviceAddress, grainFactory, logger)
    {
        _logger = logger;
        _grainAddress = deviceAddress;
    }

    protected override async Task HandlePropertiesChanged(byte[] manufacturerData, short? signalStrength)
    {
        _logger.LogDebug("Publishing ruuvi data to ruuvi actor {Mac}", DeviceAddress);
        await GrainFactory.GetGrain<IRuuviTag>(DeviceAddress).ReceiveMeasurements(
            new MeasurementEnvelope
            {
                Timestamp = DateTime.UtcNow,
                SignalStrength = signalStrength,
                Data = manufacturerData
            });
    }

    protected override async Task OnStartListening()
    {
        var ruuviTag = GrainFactory.GetGrain<IRuuviTag>(_grainAddress);
        var name = await ruuviTag.GetName();
        if (name != null)
        {
            _logger.LogInformation("Listening RuuviTag {Name} ({Mac}).", name, DeviceAddress);
        }
        else
        {
            _logger.LogInformation("New RuuviTag {Alias} found.", DeviceAddress);
        }
    }
}