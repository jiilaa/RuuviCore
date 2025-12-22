using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using net.jommy.RuuviCore.Bluez.Models;

using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class LowEnergyAdvertisingManager : BluezObject
{
    private const string Interface = "org.bluez.LEAdvertisingManager1";

    public LowEnergyAdvertisingManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task RegisterAdvertisementAsync(ObjectPath advertisement, Dictionary<string, VariantValue> options)
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
                member: "RegisterAdvertisement");
            writer.WriteObjectPath(advertisement);
            writer.WriteDictionary(options);
            return writer.CreateMessage();
        }
    }

    public Task UnregisterAdvertisementAsync(ObjectPath service)
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
                member: "UnregisterAdvertisement");
            writer.WriteObjectPath(service);
            return writer.CreateMessage();
        }
    }

    public Task<byte> GetActiveInstancesAsync()
    {
        return Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "ActiveInstances"),
            (m, s) => ReadMessage_v_y(m, (BluezObject)s!),
            this);
    }

    public Task<byte> GetSupportedInstancesAsync()
    {
        return Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "SupportedInstances"),
            (m, s) => ReadMessage_v_y(m, (BluezObject)s!),
            this);
    }

    public Task<string[]> GetSupportedIncludesAsync()
    {
        return Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "SupportedIncludes"),
            (m, s) => ReadMessage_v_as(m, (BluezObject)s!),
            this);
    }

    public Task<string[]> GetSupportedSecondaryChannelsAsync()
    {
        return Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "SupportedSecondaryChannels"),
            (m, s) => ReadMessage_v_as(m, (BluezObject)s!),
            this);
    }

    public Task<LowEnergyAdvertisingManagerProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(
            CreateGetAllPropertiesMessage(Interface),
            (m, _) => ReadMessage(m),
            this);

        static LowEnergyAdvertisingManagerProperties ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(
        Action<Exception, PropertyChanges<LowEnergyAdvertisingManagerProperties>> handler,
        bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(
            Interface,
            (m, _) => ReadMessage(m),
            handler,
            emitOnCapturedContext,
            flags);

        static PropertyChanges<LowEnergyAdvertisingManagerProperties> ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            HashSet<string> changed = [];
            return new PropertyChanges<LowEnergyAdvertisingManagerProperties>(
                ReadProperties(ref reader, changed),
                ReadInvalidated(ref reader),
                changed);
        }
    }

    private static LowEnergyAdvertisingManagerProperties ReadProperties(
        ref Reader reader,
        HashSet<string> changedList = null)
    {
        var props = new LowEnergyAdvertisingManagerProperties();
        var arrayEnd = reader.ReadArrayStart(DBusType.Struct);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            switch (property)
            {
                case "ActiveInstances":
                    reader.ReadSignature("y"u8);
                    props.ActiveInstances = reader.ReadByte();
                    changedList?.Add("ActiveInstances");
                    break;
                case "SupportedInstances":
                    reader.ReadSignature("y"u8);
                    props.SupportedInstances = reader.ReadByte();
                    changedList?.Add("SupportedInstances");
                    break;
                case "SupportedIncludes":
                    reader.ReadSignature("as"u8);
                    props.SupportedIncludes = reader.ReadArrayOfString();
                    changedList?.Add("SupportedIncludes");
                    break;
                case "SupportedSecondaryChannels":
                    reader.ReadSignature("as"u8);
                    props.SupportedSecondaryChannels = reader.ReadArrayOfString();
                    changedList?.Add("SupportedSecondaryChannels");
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}
