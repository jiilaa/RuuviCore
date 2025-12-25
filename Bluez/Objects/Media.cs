using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Bluez.Models;
using Tmds.DBus.Protocol;
// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Bluez.Objects;

public class Media : BluezObject
{
    private const string Interface = "org.bluez.Media1";

    public Media(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterEndpointAsync(ObjectPath endpoint, Dictionary<string, VariantValue> properties)
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
                member: "RegisterEndpoint");
            writer.WriteObjectPath(endpoint);
            writer.WriteDictionary(properties);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterEndpointAsync(ObjectPath endpoint)
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
                member: "UnregisterEndpoint");
            writer.WriteObjectPath(endpoint);
            return writer.CreateMessage();
        }
    }

    public Task RegisterPlayerAsync(ObjectPath player, Dictionary<string, VariantValue> properties)
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
                member: "RegisterPlayer");
            writer.WriteObjectPath(player);
            writer.WriteDictionary(properties);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterPlayerAsync(ObjectPath player)
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
                member: "UnregisterPlayer");
            writer.WriteObjectPath(player);
            return writer.CreateMessage();
        }
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

    public Task<IReadOnlyCollection<string>> GetSupportedUUIDsAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "SupportedUUIDs"), (m, s) => ReadMessage_v_as(m), this);
    }

    public Task<MediaProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(CreateGetAllPropertiesMessage(Interface), (m, _) => ReadMessage(m), this);

        static MediaProperties ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception, PropertyChanges<MediaProperties>> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(Interface, (m, _) => ReadMessage(m), handler, emitOnCapturedContext, flags);

        static PropertyChanges<MediaProperties> ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            HashSet<string> changed = [];
            return new PropertyChanges<MediaProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed);
        }
    }

    private static MediaProperties ReadProperties(ref Reader reader, HashSet<string> changedList = null)
    {
        var props = new MediaProperties();
        var arrayEnd = reader.ReadArrayStart(DBusType.Struct);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            switch (property)
            {
                case "SupportedUUIDs":
                    reader.ReadSignature("as"u8);
                    props.SupportedUUIDs = reader.ReadArrayOfString();
                    changedList?.Add("SupportedUUIDs");
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}
