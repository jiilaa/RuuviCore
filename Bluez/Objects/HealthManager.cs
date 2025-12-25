using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class HealthManager : BluezObject
{
    private const string Interface = "org.bluez.HealthManager1";

    public HealthManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task<ObjectPath> CreateApplicationAsync(Dictionary<string, VariantValue> config)
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_o(m), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "a{sv}",
                member: "CreateApplication");
            writer.WriteDictionary(config);
            return writer.CreateMessage();
        }
    }

    public Task DestroyApplicationAsync(ObjectPath application)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "o",
                member: "DestroyApplication");
            writer.WriteObjectPath(application);
            return writer.CreateMessage();
        }
    }
}