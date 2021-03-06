﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using bluez.DBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Concurrency;
using Orleans.Core;
using Orleans.Runtime;
using Tmds.DBus;

namespace net.jommy.RuuviCore.GrainServices
{
    /// <summary>
    /// A service which listens to bluetooth packets from DBUS. 
    /// </summary>
    [Reentrant]
    public class DBusListener : GrainService, IRuuviDBusListener
    {
        private const string DBusServiceName = "org.bluez";
        private const string DBusDeviceInterfaceName = "org.bluez.Device1";
        private readonly IDictionary<string, IDeviceListener> _deviceListeners = new ConcurrentDictionary<string, IDeviceListener>();
        private IDisposable _interfacesAddedWatcher;
        private IDisposable _propertyChangeWatcher;
        private IAdapter1 _adapter;
        private readonly DBusSettings _dbusSettings;
        private readonly DeviceListenerFactory _deviceListenerFactory;
        private readonly ILogger<DBusListener> _logger;

        public DBusListener(IGrainIdentity grainId, Silo silo, ILoggerFactory loggerFactory, IGrainFactory grainFactory, IOptions<DBusSettings> dbusOptions) : base(grainId, silo, loggerFactory)
        {
            _dbusSettings = dbusOptions.Value;
            _deviceListenerFactory = new DeviceListenerFactory(grainFactory, loggerFactory);
            _logger = loggerFactory.CreateLogger<DBusListener>();
        }

        public override async Task Stop()
        {
            await _adapter.StopDiscoveryAsync();
            
            foreach (var deviceListener in _deviceListeners)
            {
                deviceListener.Value.Dispose();
            }
            _interfacesAddedWatcher.Dispose();
            _propertyChangeWatcher.Dispose();
            
            await base.Stop();
        }

        public override Task Start()
        {
            Task.Factory.StartNew(MainLoop);

            return base.Start();
        }

        private async void MainLoop()
        {
            _logger.LogInformation("Starting Ruuvi DBUS Listener");

            try
            {
                _adapter = Connection.System.CreateProxy<IAdapter1>(DBusServiceName, $"/org/bluez/{_dbusSettings.BluetoothAdapterName}");
                var objectManager = Connection.System.CreateProxy<IObjectManager>(DBusServiceName, "/");
                _interfacesAddedWatcher = await objectManager.WatchInterfacesAddedAsync(OnDeviceAdded);
                _propertyChangeWatcher = await _adapter.WatchPropertiesAsync(OnPropertyChanges);
             
                var objects = await objectManager.GetManagedObjectsAsync();
                foreach (var device in objects)
                {
                    var objectPath = device.Key.ToString();
                    // First make sure the device is under the correct parent (i.e. the adapter)
                    if (!objectPath.StartsWith(_adapter.ObjectPath.ToString()))
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
                _logger.LogWarning(e, "DBUS error: {errorMessage}", e.Message);
                _logger.LogInformation("Ruuvi DBUS Listener NOT listening for bluetooth events.");
            }
        }

        private void OnPropertyChanges(PropertyChanges obj)
        {
            // TODO: This is probably no longer needed, test if DBUS listening works without this.  
        }

        private async void OnDeviceAdded((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
        {
            // Async void is a big no-no, but Tmds.DBus doesn't provide a Task based way to do this,
            // so pokemon-catch all exceptions to avoid crashing the whole application
            try
            {
                await RegisterDevice(args.objectPath);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Error occurred when discovering a device: {errorMessage}.", e.Message);
            }
        }

        private async Task RegisterDevice(ObjectPath objectPath)
        {
            var device = Connection.System.CreateProxy<IDevice1>(DBusServiceName, objectPath);
            IDictionary<ushort, object> manufacturerData;
            try
            {
                manufacturerData = await device.GetManufacturerDataAsync();
            }
            catch (DBusException e)
            {
                // If manufacturer data property is not found, Tmds throws exception with details:
                // org.freedesktop.DBus.Error.InvalidArgs: No such property 'ManufacturerData'
                if (e.ErrorName.EndsWith("InvalidArgs"))
                {
                    _logger.LogDebug("Skipping device without manufacturer data.");
                    return;
                }
                throw;
            }
            
            if (manufacturerData != null)
            {
                var address = await device.GetAddressAsync();
                // It seems sometimes devices are found again, so let's dispose old observers so DBUS limits will not get exceeded.
                if (_deviceListeners.Remove(address, out var oldListener))
                {
                    _logger.LogDebug("Disposing old observer.", address);
                    oldListener.Dispose();
                }

                if (_deviceListenerFactory.TryConstructDeviceListener(device, address, manufacturerData, out var deviceListener))
                {
                    _deviceListeners[address] = deviceListener;
                    await deviceListener.StartListeningAsync();
                }
                else
                {
                    _logger.LogDebug("Unsupported manufacturer, ignoring.");
                }
            }
            else
            {
                _logger.LogDebug("Discarding a bluetooth device without manufacturer data.");
            }            
        }
    }
}
