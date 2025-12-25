using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class ProfileManager : BluezObject
{
    private const string Interface = "org.bluez.ProfileManager1";

    public ProfileManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterProfileAsync(ObjectPath profile, string uUID, Dictionary<string, VariantValue> options)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "osa{sv}",
                member: "RegisterProfile");
            writer.WriteObjectPath(profile);
            writer.WriteString(uUID);
            writer.WriteDictionary(options);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterProfileAsync(ObjectPath profile)
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
                member: "UnregisterProfile");
            writer.WriteObjectPath(profile);
            return writer.CreateMessage();
        }
    }
}