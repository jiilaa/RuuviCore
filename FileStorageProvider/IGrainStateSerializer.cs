using Newtonsoft.Json.Linq;
using Orleans;

namespace net.jommy.Orleans
{
    public interface IGrainStateSerializer
    {
        JObject Serialize(IGrainState grainState);

        void Deserialize(IGrainState grainState, JObject entityData);
    }
}
