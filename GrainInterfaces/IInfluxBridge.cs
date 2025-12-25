using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface IInfluxBridge : IGrainWithIntegerKey
{
    Task<bool> WriteMeasurements(string macAddress, string name, MeasurementDTO measurements);
}