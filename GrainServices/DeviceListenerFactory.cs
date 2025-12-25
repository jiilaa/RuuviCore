using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Bluez.Objects;
using Orleans;
using Tmds.DBus.Protocol;

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

    public bool TryConstructDeviceListener(
        Device device, 
        string deviceAddress, 
        Dictionary<ushort, VariantValue> manufacturerData, 
        out IDeviceListener deviceListener)
    {
        deviceListener = null;
        if (manufacturerData.ContainsKey(RuuviManufacturerKey))
        {
            deviceListener = new RuuviTagListener(
                device, 
                deviceAddress, 
                _grainFactory, 
                _loggerFactory.CreateLogger<RuuviTagListener>());
        }

        return deviceListener != null;
    }
}