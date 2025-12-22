using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.jommy.RuuviCore.Bluez;
using net.jommy.RuuviCore.Bluez.Objects;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.GrainServices;

/// <summary>
/// A service which listens to bluetooth packets from DBUS. 
/// </summary>
[Reentrant]
public class DBusListener : GrainService, IRuuviDBusListener
{
    private readonly IGrainFactory _grainFactory;
    private const string DBusServiceName = "org.bluez";
    private const string DBusDeviceInterfaceName = "org.bluez.Device1";
    private readonly IDictionary<string, IDeviceListener> _deviceListeners = new ConcurrentDictionary<string, IDeviceListener>();
    private IDisposable _interfacesAddedWatcher;
    private Connection _connection;
    private BluezObjectFactory _factory;
    private Adapter _adapter;
    private readonly DBusSettings _dbusSettings;
    private readonly DeviceListenerFactory _deviceListenerFactory;
    private readonly ILogger<DBusListener> _logger;

    public DBusListener(GrainId grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory, IOptions<DBusSettings> dbusOptions) 
        : base(grainId, silo, loggerFactory)
    {
        _grainFactory = grainFactory;
        _dbusSettings = dbusOptions.Value;
        _deviceListenerFactory = new DeviceListenerFactory(grainFactory, loggerFactory);
        _logger = loggerFactory.CreateLogger<DBusListener>();
    }

    public override async Task Stop()
    {
        if (_adapter != null)
        {
            await _adapter.StopDiscoveryAsync();
        }

        foreach (var deviceListener in _deviceListeners)
        {
            deviceListener.Value.Dispose();
        }
        _interfacesAddedWatcher?.Dispose();
        _connection?.Dispose();

        await base.Stop();
    }

    public override Task Start()
    {
        Task.Factory.StartNew(MainLoop);

        return base.Start();
    }

    private async void MainLoop()
    {
        if (_dbusSettings.BluetoothAdapterName == DBusSettings.SimulatedAdapterName)
        {
            _logger.LogInformation("Simulating Ruuvi DBUS Listener");
            foreach (var simulatedDevice in _dbusSettings.SimulatedDevices)
            {
                await _grainFactory.GetGrain<ISimulatedTag>(simulatedDevice).Start();
            }
            return;
        }
        _logger.LogInformation("Starting Ruuvi DBUS Listener");

        try
        {
            // Create connection to system bus
            _connection = new Connection(Address.System);
            await _connection.ConnectAsync();

            // Create factory for Bluez objects
            _factory = new BluezObjectFactory(_connection, DBusServiceName);

            // Create adapter and object manager
            _adapter = _factory.CreateAdapter(new ObjectPath($"/org/bluez/{_dbusSettings.BluetoothAdapterName}"));
            var objectManager = _factory.CreateObjectManager(new ObjectPath("/"));

            // Watch for new devices and property changes
            _interfacesAddedWatcher = await objectManager.WatchInterfacesAddedAsync(OnDeviceAdded);
             
            var objects = await objectManager.GetManagedObjectsAsync();
            foreach (var device in objects)
            {
                var objectPath = device.Key;
                var objectPathString = objectPath.ToString();
                // First make sure the device is under the correct parent (i.e. the adapter)
                if (!objectPathString.StartsWith(_adapter.Path.ToString()))
                {
                    continue;
                }

                // Then check the interface
                if (device.Value.ContainsKey(DBusDeviceInterfaceName))
                {
                    await RegisterDevice(objectPath);
                }
            }
                
            await _adapter.StartDiscoveryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "DBUS error: {errorMessage}", e.Message);
            _logger.LogError("Ruuvi DBUS Listener NOT listening for bluetooth events.");
        }
    }

    private async void OnDeviceAdded(Exception ex, (ObjectPath objectPath, Dictionary<string, Dictionary<string, VariantValue>> interfaces) args)
    {
        // Async void is a big no-no, but Tmds.DBus doesn't provide a Task based way to do this,
        // so pokemon-catch all exceptions to avoid crashing the whole application
        try
        {
            if (ex != null)
            {
                _logger.LogError(ex, "Error in InterfacesAdded signal");
                return;
            }
            await RegisterDevice(args.objectPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error occurred when discovering a device: {errorMessage}.", e.Message);
        }
    }

    private async Task RegisterDevice(ObjectPath objectPath)
    {
        _logger.LogInformation("Registering device: {objectPath}", objectPath);

        var device = _factory.CreateDevice(objectPath);
        Dictionary<ushort, VariantValue> manufacturerData;
        try
        {
            manufacturerData = await device.GetManufacturerDataAsync();
        }
        catch (DBusException e)
        {
            // If manufacturer data property is not found, Tmds throws exception with details:
            // org.freedesktop.DBus.Error.InvalidArgs: No such property 'ManufacturerData'
            if (e.ErrorName != null && e.ErrorName.EndsWith("InvalidArgs"))
            {
                _logger.LogDebug("Skipping device without manufacturer data.");
                return;
            }

            _logger.LogDebug("Error getting manufacturer data: {ErrorMessage}. Continuing.", e.Message);
            return;
        }
        catch (Exception e)
        {
            _logger.LogError("A non-DBUS exception occurred: {ErrorMessage}. Continuing.", e.Message);
            return;
        }
            
        if (manufacturerData != null)
        {
            var address = await device.GetAddressAsync();

            if (_deviceListeners.TryGetValue(address, out var existingListener))
            {
                if (existingListener.IsAlive())
                {
                    _logger.LogDebug("Using old device listener with address {address} to handle manufacturer data.", address);
                    await existingListener.HandleDataAsync(manufacturerData);
                    return;
                }

                // Devices are found again with certain interval. If old listener hasn't had data for a while, let's dispose it and start a new one.
                _logger.LogInformation("Disposing old device listener with address {Address}.", address);
                _deviceListeners.Remove(address);
                existingListener.Dispose();
            }
                
            if (_deviceListenerFactory.TryConstructDeviceListener(device, address, manufacturerData, out var deviceListener))
            {
                _deviceListeners[address] = deviceListener;
                await deviceListener.StartListeningAsync();
                await deviceListener.HandleDataAsync(manufacturerData);
            }
            else
            {
                _logger.LogDebug("Unsupported manufacturer, ignoring: {Data}.", manufacturerData);
            }
        }
        else
        {
            _logger.LogDebug("Discarding a bluetooth device without manufacturer data.");
        }            
    }

    public async Task SimulateEvent(string macAddress)
    {
        await _grainFactory.GetGrain<IRuuviTag>(macAddress).ReceiveMeasurements(
            new MeasurementEnvelope
            {
                Timestamp = DateTime.UtcNow,
                SignalStrength = (short)Random.Shared.Next(),
                Data = null
            });
    }
}