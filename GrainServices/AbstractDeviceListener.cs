using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using net.jommy.RuuviCore.Bluez.Models;
using net.jommy.RuuviCore.Bluez.Objects;
using Orleans;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.GrainServices;

public abstract class AbstractDeviceListener : IDeviceListener
{
    private const int AliveThreshold = 60;
    private const string ManufacturerDataKeyName = "ManufacturerData";

    private readonly Device _device;
    private IDisposable _propertiesWatcher;
    private int _aliveCounter;

    protected readonly string DeviceAddress;
    protected readonly IGrainFactory GrainFactory;
    private readonly ILogger _logger;

    protected abstract Task HandlePropertiesChanged(byte[] manufacturerData, short? signalStrength);

    protected abstract Task OnStartListening();
        
    protected abstract ushort ManufacturerKey { get; }

    protected AbstractDeviceListener(Device device, string deviceAddress, IGrainFactory grainFactory, ILogger logger)
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
        _propertiesWatcher = await _device.WatchPropertiesChangedAsync(OnPropertiesChanged);
    }

    public bool IsAlive()
    {
        _aliveCounter++;
        return _aliveCounter <= AliveThreshold;
    }

    private async Task<short?> GetSignalStrength()
    {
        try
        {
            return await _device.GetRSSIAsync();
        }
        catch (DBusException ex) when (ex.ErrorName?.Contains("InvalidArgs") == true)
        {
            // RSSI property not available yet (common during initial device discovery)
            _logger.LogDebug("RSSI not available for device {DeviceAddress}: {ErrorMessage}", DeviceAddress, ex.Message);
            return null;
        }
    }

    public async Task HandleDataAsync(Dictionary<ushort, VariantValue> manufacturerData)
    {
        if (manufacturerData.TryGetValue(ManufacturerKey, out var variantValue))
        {
            var bytes = variantValue.GetArray<byte>();
            var signalStrength = await GetSignalStrength();
            await HandlePropertiesChanged(bytes, signalStrength);
        }
    }

    private async void OnPropertiesChanged(Exception ex, PropertyChanges<DeviceProperties> changes)
    {
        // Async void is a big no-no, but Tmds.DBus doesn't provide a Task based way to do this,
        // so pokemon-catch all exceptions to avoid crashing the whole application

        try
        {
            if (ex != null)
            {
                _logger.LogError(ex, "Error in property change for device {DeviceAddress}: {ErrorMessage}", DeviceAddress, ex.Message);
                return;
            }

            _aliveCounter = 0;
            _logger.LogDebug("Property change received for device {DeviceAddress}", DeviceAddress);

            // Check if ManufacturerData property changed
            if (!changes.HasChanged(ManufacturerDataKeyName))
            {
                _logger.LogDebug("Property change for {DeviceAddress} was not ManufacturerData, ignoring", DeviceAddress);
                return;
            }

            var manufacturerData = changes.Properties.ManufacturerData;
            if (manufacturerData != null && manufacturerData.TryGetValue(ManufacturerKey, out var variantValue))
            {
                var signalStrength = await GetSignalStrength();
                var bytes = variantValue.GetArray<byte>();
                await HandlePropertiesChanged(bytes, signalStrength);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to handle properties changed event for device {DeviceAddress}: {Error}", DeviceAddress, e.Message);
        }
    }
}