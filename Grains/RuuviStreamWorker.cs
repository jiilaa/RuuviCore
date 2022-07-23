using System.Threading.Tasks;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;
using Orleans;
using Orleans.Concurrency;

namespace net.jommy.RuuviCore.Grains
{
    [StatelessWorker]
    public class RuuviStreamWorker : Grain, IRuuviStreamWorker
    {
        public async Task Publish(string target, MeasurementEnvelope envelope)
        {
            var streamProvider = GetStreamProvider(RuuviCoreConstants.StreamProviderName);
            var stream = streamProvider.GetStream<MeasurementEnvelope>(target.ToActorGuid(), "MeasurementStream");
            await stream.OnNextAsync(envelope);
        }
    }
}