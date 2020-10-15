using System.Collections.Generic;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Interfaces;
using Orleans;

namespace net.jommy.RuuviCore.Services
{
    public class RuuviTagRepository : IRuuviTagRepository
    {
        private readonly IClusterClient _clusterClient;

        public RuuviTagRepository(IClusterClient clusterClient)
        {
            _clusterClient = clusterClient;
        }

        public Task<List<RuuviTagInfo>> GetRuuviTagsAsync()
        {
            return _clusterClient.GetGrain<IRuuviTagRegistry>(0)
                .GetAll();
        }
    }
}