using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans;
using Orleans.Runtime;
using Orleans.Serialization;

namespace net.jommy.Orleans
{
    public class JsonGrainStateSerializer : IGrainStateSerializer
    {
        private readonly JsonSerializer _serializer;

        public JsonGrainStateSerializer(ITypeResolver typeResolver, IGrainFactory grainFactory)
        {
            var jsonSettings = OrleansJsonSerializer.GetDefaultSerializerSettings(typeResolver, grainFactory);

            _serializer = JsonSerializer.Create(jsonSettings);

            _serializer.NullValueHandling = NullValueHandling.Include;
            _serializer.DefaultValueHandling = DefaultValueHandling.Populate;
        }

        public void Deserialize(IGrainState grainState, JObject entityData)
        {
            var jsonReader = new JTokenReader(entityData);

            _serializer.Populate(jsonReader, grainState.State);
        }

        public JObject Serialize(IGrainState grainState)
        {
            return JObject.FromObject(grainState.State, _serializer);
        }
    }
}
