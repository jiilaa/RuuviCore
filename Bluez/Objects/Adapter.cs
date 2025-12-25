using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Bluez.Models;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class Adapter : BluezObject
{
    private const string Interface = "org.bluez.Adapter1";

    public Adapter(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task StartDiscoveryAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                "StartDiscovery");
            return writer.CreateMessage();
        }
    }

    public Task SetDiscoveryFilterAsync(Dictionary<string, VariantValue> properties)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                signature: "a{sv}",
                member: "SetDiscoveryFilter");
            writer.WriteDictionary(properties);
            return writer.CreateMessage();
        }
    }

    public Task StopDiscoveryAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                "StopDiscovery");
            return writer.CreateMessage();
        }
    }

    public Task RemoveDeviceAsync(ObjectPath device)
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
                member: "RemoveDevice");
            writer.WriteObjectPath(device);
            return writer.CreateMessage();
        }
    }

    public Task<string[]> GetDiscoveryFiltersAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_as(m), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                "GetDiscoveryFilters");
            return writer.CreateMessage();
        }
    }

    public Task SetAliasAsync(string value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Alias");
            writer.WriteSignature("s");
            writer.WriteString(value);
            return writer.CreateMessage();
        }
    }

    public Task SetPoweredAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Powered");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task SetDiscoverableAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Discoverable");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task SetDiscoverableTimeoutAsync(uint value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("DiscoverableTimeout");
            writer.WriteSignature("u");
            writer.WriteUInt32(value);
            return writer.CreateMessage();
        }
    }

    public Task SetPairableAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Pairable");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task SetPairableTimeoutAsync(uint value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("PairableTimeout");
            writer.WriteSignature("u");
            writer.WriteUInt32(value);
            return writer.CreateMessage();
        }
    }

    public Task<string> GetAddressAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Address"), (m, s) => ReadMessage_v_s(m), this);
    }

    public Task<string> GetAddressTypeAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "AddressType"), (m, s) => ReadMessage_v_s(m), this);
    }

    public Task<string> GetNameAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Name"), (m, s) => ReadMessage_v_s(m), this);
    }

    public Task<string> GetAliasAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Alias"), (m, s) => ReadMessage_v_s(m), this);
    }

    public Task<uint> GetClassAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Class"), (m, s) => ReadMessage_v_u(m), this);
    }

    public Task<bool> GetPoweredAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Powered"), (m, s) => ReadMessage_v_b(m), this);
    }

    public Task<bool> GetDiscoverableAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Discoverable"), (m, s) => ReadMessage_v_b(m), this);
    }

    public Task<uint> GetDiscoverableTimeoutAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "DiscoverableTimeout"), (m, s) => ReadMessage_v_u(m), this);
    }

    public Task<bool> GetPairableAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Pairable"), (m, s) => ReadMessage_v_b(m), this);
    }

    public Task<uint> GetPairableTimeoutAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "PairableTimeout"), (m, s) => ReadMessage_v_u(m), this);
    }

    public Task<bool> GetDiscoveringAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Discovering"), (m, s) => ReadMessage_v_b(m), this);
    }

    public Task<IReadOnlyCollection<string>> GetUUIDsAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "UUIDs"), (m, s) => ReadMessage_v_as(m), this);
    }

    public Task<string> GetModaliasAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Modalias"), (m, s) => ReadMessage_v_s(m), this);
    }

    public Task<IReadOnlyCollection<string>> GetRolesAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "Roles"), (m, s) => ReadMessage_v_as(m), this);
    }

    public Task<IReadOnlyCollection<string>> GetExperimentalFeaturesAsync()
    {
        return Connection.CallMethodAsync(CreateGetPropertyMessage(Interface, "ExperimentalFeatures"), (m, s) => ReadMessage_v_as(m), this);
    }

    public Task<AdapterProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(CreateGetAllPropertiesMessage(Interface), (m, _) => ReadMessage(m), this);

        static AdapterProperties ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(Action<Exception, PropertyChanges<AdapterProperties>> handler, bool emitOnCapturedContext = true, ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(Interface, (m, _) => ReadMessage(m), handler, emitOnCapturedContext, flags);

        static PropertyChanges<AdapterProperties> ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            HashSet<string> changed = [];
            return new PropertyChanges<AdapterProperties>(ReadProperties(ref reader, changed), ReadInvalidated(ref reader), changed);
        }
    }

    private static AdapterProperties ReadProperties(ref Reader reader, HashSet<string> changedList = null)
    {
        var props = new AdapterProperties();
        var arrayEnd = reader.ReadArrayStart(DBusType.Struct);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            changedList?.Add(property);
            switch (property)
            {
                case "Address":
                    reader.ReadSignature("s"u8);
                    props.Address = reader.ReadString();
                    break;
                case "AddressType":
                    reader.ReadSignature("s"u8);
                    props.AddressType = reader.ReadString();
                    break;
                case "Name":
                    reader.ReadSignature("s"u8);
                    props.Name = reader.ReadString();
                    break;
                case "Alias":
                    reader.ReadSignature("s"u8);
                    props.Alias = reader.ReadString();
                    break;
                case "Class":
                    reader.ReadSignature("u"u8);
                    props.Class = reader.ReadUInt32();
                    break;
                case "Powered":
                    reader.ReadSignature("b"u8);
                    props.Powered = reader.ReadBool();
                    break;
                case "Discoverable":
                    reader.ReadSignature("b"u8);
                    props.Discoverable = reader.ReadBool();
                    break;
                case "DiscoverableTimeout":
                    reader.ReadSignature("u"u8);
                    props.DiscoverableTimeout = reader.ReadUInt32();
                    break;
                case "Pairable":
                    reader.ReadSignature("b"u8);
                    props.Pairable = reader.ReadBool();
                    break;
                case "PairableTimeout":
                    reader.ReadSignature("u"u8);
                    props.PairableTimeout = reader.ReadUInt32();
                    break;
                case "Discovering":
                    reader.ReadSignature("b"u8);
                    props.Discovering = reader.ReadBool();
                    break;
                case "UUIDs":
                    reader.ReadSignature("as"u8);
                    props.UUIDs = reader.ReadArrayOfString();
                    break;
                case "Modalias":
                    reader.ReadSignature("s"u8);
                    props.Modalias = reader.ReadString();
                    break;
                case "Roles":
                    reader.ReadSignature("as"u8);
                    props.Roles = reader.ReadArrayOfString();
                    break;
                case "ExperimentalFeatures":
                    reader.ReadSignature("as"u8);
                    props.ExperimentalFeatures = reader.ReadArrayOfString();
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}
