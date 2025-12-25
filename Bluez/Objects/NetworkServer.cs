using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class NetworkServer : BluezObject
{
    private const string Interface = "org.bluez.NetworkServer1";

    public NetworkServer(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterAsync(string uuid, string bridge)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "ss",
                member: "Register");
            writer.WriteString(uuid);
            writer.WriteString(bridge);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterAsync(string uuid)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "s",
                member: "Unregister");
            writer.WriteString(uuid);
            return writer.CreateMessage();
        }
    }
}