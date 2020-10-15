using System;
using System.Threading.Tasks;

namespace net.jommy.RuuviCore.GrainServices
{
    public interface IDeviceListener : IDisposable
    {
        Task StartListeningAsync();
    }
}