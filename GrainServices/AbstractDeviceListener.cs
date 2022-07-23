using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bluez.DBus;
using Orleans;
using Tmds.DBus;

namespace net.jommy.RuuviCore.GrainServices
{
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

        protected abstract Task HandlePropertiesChanged(byte[] manufacturerData, short? signalStrength);

        protected abstract Task OnStartListening();
        
        protected abstract ushort ManufacturerKey { get; }

        protected AbstractDeviceListener(IDevice1 device, string deviceAddress, IGrainFactory grainFactory)
        {
            _device = device;
            DeviceAddress = deviceAddress;
            GrainFactory = grainFactory;
        }

        /// <inheritdoc />
        public void Dispose()
        {
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

        protected Task<short> GetSignalStrength()
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
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
