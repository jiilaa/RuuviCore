using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class GattManager : BluezObject
{
    private const string Interface = "org.bluez.GattManager1";

    public GattManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterApplicationAsync(ObjectPath application, Dictionary<string, VariantValue> options)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "oa{sv}",
                member: "RegisterApplication");
            writer.WriteObjectPath(application);
            writer.WriteDictionary(options);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterApplicationAsync(ObjectPath application)
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
                member: "UnregisterApplication");
            writer.WriteObjectPath(application);
            return writer.CreateMessage();
        }
    }
}