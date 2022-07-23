using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace net.jommy.RuuviCore.GrainServices
{
    public interface IDeviceListener : IDisposable
    {
        Task StartListeningAsync();

        bool IsAlive();

        Task HandleDataAsync(IDictionary<ushort, object> manufacturerData);
    }
}