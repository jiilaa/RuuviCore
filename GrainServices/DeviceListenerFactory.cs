using System.Collections.Generic;
using bluez.DBus;
using Microsoft.Extensions.Logging;
using Orleans;

namespace net.jommy.RuuviCore.GrainServices;

public class DeviceListenerFactory
{
    private readonly IGrainFactory _grainFactory;
    private readonly ILoggerFactory _loggerFactory;
    private const ushort RuuviManufacturerKey = 1177;

    public DeviceListenerFactory(IGrainFactory grainFactory, ILoggerFactory loggerFactory)
    {
        _grainFactory = grainFactory;
        _loggerFactory = loggerFactory;
    }

    public bool TryConstructDeviceListener(IDevice1 device, string deviceAddress, IDictionary<ushort, object> manufacturerData, out IDeviceListener deviceListener)
    {
        deviceListener = null;
        if (manufacturerData.ContainsKey(RuuviManufacturerKey))
        {
            deviceListener = new RuuviTagListener(device, deviceAddress, _grainFactory, _loggerFactory.CreateLogger<RuuviTagListener>());
        }

        return deviceListener != null;
    }
}