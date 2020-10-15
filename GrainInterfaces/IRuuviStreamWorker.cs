using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces
{
    public interface IRuuviStreamWorker : IGrainWithIntegerKey
    {
        Task Publish(string target, MeasurementEnvelope envelope);
    }
}