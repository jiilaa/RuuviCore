using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class AgentManager : BluezObject
{
    private const string Interface = "org.bluez.AgentManager1";

    public AgentManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterAgentAsync(ObjectPath agent, string capability)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "os",
                member: "RegisterAgent");
            writer.WriteObjectPath(agent);
            writer.WriteString(capability);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterAgentAsync(ObjectPath agent)
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
                member: "UnregisterAgent");
            writer.WriteObjectPath(agent);
            return writer.CreateMessage();
        }
    }

    public Task RequestDefaultAgentAsync(ObjectPath agent)
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
                member: "RequestDefaultAgent");
            writer.WriteObjectPath(agent);
            return writer.CreateMessage();
        }
    }
}