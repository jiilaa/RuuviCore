using System.Collections.Generic;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Services
{
    public interface IRuuviTagRepository
    {
        Task<List<RuuviTagInfo>> GetRuuviTagsAsync();
    }
}