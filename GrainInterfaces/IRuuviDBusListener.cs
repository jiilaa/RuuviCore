using System.Threading.Tasks;
using Orleans.Services;

namespace net.jommy.RuuviCore.Interfaces;

public interface IRuuviDBusListener : IGrainService
{
    [Orleans.Alias("SimulateEvent")]
    Task SimulateEvent(string macAddress);
}