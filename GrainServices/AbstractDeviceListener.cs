using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using bluez.DBus;
using Orleans;
using Tmds.DBus;

namespace net.jommy.RuuviCore.GrainServices
{
    public abstract class AbstractDeviceListener : IDeviceListener
    {
        private const string SignalStrengthKeyName = "RSSI";
        private const string ManufacturerDataKeyName = "ManufacturerData";

        private readonly IDevice1 _device;
        private IDisposable _propertiesWatcher;

        protected readonly string DeviceAddress;
        protected readonly IGrainFactory GrainFactory;

        protected abstract Task HandlePropertiesChanged(byte[] manufacturerData);
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

        protected Task<short> GetSignalStrength()
        {
            return _device.GetAsync<short>(SignalStrengthKeyName);
        }

        private async void OnPropertiesChanged(PropertyChanges changes)
        {
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
                    await HandlePropertiesChanged((byte[]) dict[ManufacturerKey]);
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
