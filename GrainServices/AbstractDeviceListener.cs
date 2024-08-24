using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bluez.DBus;
using Microsoft.Extensions.Logging;
using Orleans;
using Tmds.DBus;

namespace net.jommy.RuuviCore.GrainServices;

public abstract class AbstractDeviceListener : IDeviceListener
{
    private const int AliveThreshold = 60; 
    private const string SignalStrengthKeyName = "RSSI";
    private const string ManufacturerDataKeyName = "ManufacturerData";

    private readonly IDevice1 _device;
    private IDisposable _propertiesWatcher;
    private int _aliveCounter;

    protected readonly string DeviceAddress;
    protected readonly IGrainFactory GrainFactory;
    private readonly ILogger _logger;

    protected abstract Task HandlePropertiesChanged(byte[] manufacturerData, short? signalStrength);

    protected abstract Task OnStartListening();
        
    protected abstract ushort ManufacturerKey { get; }

    protected AbstractDeviceListener(IDevice1 device, string deviceAddress, IGrainFactory grainFactory, ILogger logger)
    {
        _device = device;
        DeviceAddress = deviceAddress;
        GrainFactory = grainFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _propertiesWatcher?.Dispose();
    }

    /// <inheritdoc />
    public async Task StartListeningAsync()
    {
        await OnStartListening();
        _propertiesWatcher = await _device.WatchPropertiesAsync(OnPropertiesChanged);
    }

    public bool IsAlive()
    {
        _aliveCounter++;
        return _aliveCounter <= AliveThreshold;
    }

    private Task<short> GetSignalStrength()
    {
        return _device.GetAsync<short>(SignalStrengthKeyName);
    }

    public async Task HandleDataAsync(IDictionary<ushort, object> manufacturerData)
    {
        if (manufacturerData.TryGetValue(ManufacturerKey, out var bytes))
        {
            await HandlePropertiesChanged((byte[])bytes, null);
        }
    }

    private async void OnPropertiesChanged(PropertyChanges changes)
    {
        _aliveCounter = 0;
        try
        {
            var manufacturerDataChange = changes.Changed.FirstOrDefault(c => c.Key == ManufacturerDataKeyName);
            if (manufacturerDataChange.Value == null)
            {
                return;
            }

            var dict = (IDictionary)manufacturerDataChange.Value;
            if (dict.Contains(ManufacturerKey))
            {
                var signalStrength = await GetSignalStrength();
                await HandlePropertiesChanged((byte[]) dict[ManufacturerKey], signalStrength);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle properties changed event for device {deviceAddress}: {error}", DeviceAddress, e.Message);
        }
    }
}