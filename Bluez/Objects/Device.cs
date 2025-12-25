using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using net.jommy.RuuviCore.Bluez.Models;

using Tmds.DBus.Protocol;

// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Bluez.Objects;

public class Device : BluezObject
{
    private const string Interface = "org.bluez.Device1";

    public Device(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task DisconnectAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                member: "Disconnect");
            return writer.CreateMessage();
        }
    }

    public Task ConnectAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                member: "Connect");
            return writer.CreateMessage();
        }
    }

    public Task ConnectProfileAsync(string uUID)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                signature: "s",
                member: "ConnectProfile");
            writer.WriteString(uUID);
            return writer.CreateMessage();
        }
    }

    public Task DisconnectProfileAsync(string uUID)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                signature: "s",
                member: "DisconnectProfile");
            writer.WriteString(uUID);
            return writer.CreateMessage();
        }
    }

    public Task PairAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                member: "Pair");
            return writer.CreateMessage();
        }
    }

    public Task CancelPairingAsync()
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: Interface,
                member: "CancelPairing");
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
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Alias");
            writer.WriteSignature("s");
            writer.WriteString(value);
            return writer.CreateMessage();
        }
    }

    public Task SetTrustedAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Trusted");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task SetBlockedAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("Blocked");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task SetWakeAllowedAsync(bool value)
    {
        return Connection.CallMethodAsync(CreateMessage());

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                destination: ObjectFactory.Destination,
                path: Path,
                @interface: "org.freedesktop.DBus.Properties",
                signature: "ssv",
                member: "Set");
            writer.WriteString(Interface);
            writer.WriteString("WakeAllowed");
            writer.WriteSignature("b");
            writer.WriteBool(value);
            return writer.CreateMessage();
        }
    }

    public Task<string> GetAddressAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Address"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<string> GetAddressTypeAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "AddressType"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<string> GetNameAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Name"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<string> GetAliasAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Alias"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<uint> GetClassAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Class"),
            (m, s) => ReadMessage_v_u(m),
            this);

    public Task<ushort> GetAppearanceAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Appearance"),
            (m, s) => ReadMessage_v_q(m),
            this);

    public Task<string> GetIconAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Icon"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<bool> GetPairedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Paired"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<bool> GetBondedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Bonded"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<bool> GetTrustedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Trusted"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<bool> GetBlockedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Blocked"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<bool> GetLegacyPairingAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "LegacyPairing"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<short> GetRSSIAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "RSSI"),
            (m, s) => ReadMessage_v_n(m),
            this);

    public Task<bool> GetConnectedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Connected"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<IReadOnlyCollection<string>> GetUUIDsAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "UUIDs"),
            (m, s) => ReadMessage_v_as(m),
            this);

    public Task<string> GetModaliasAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Modalias"),
            (m, s) => ReadMessage_v_s(m),
            this);

    public Task<ObjectPath> GetAdapterAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "Adapter"),
            (m, s) => ReadMessage_v_o(m),
            this);

    public Task<Dictionary<ushort, VariantValue>> GetManufacturerDataAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "ManufacturerData"),
            (m, s) => ReadMessage_v_aeqv(m),
            this);

    public Task<Dictionary<string, VariantValue>> GetServiceDataAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "ServiceData"),
            (m, s) => ReadMessage_v_aesv(m),
            this);

    public Task<short> GetTxPowerAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "TxPower"),
            (m, s) => ReadMessage_v_n(m),
            this);

    public Task<bool> GetServicesResolvedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "ServicesResolved"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<bool> GetWakeAllowedAsync()
        => Connection.CallMethodAsync(
            CreateGetPropertyMessage(Interface, "WakeAllowed"),
            (m, s) => ReadMessage_v_b(m),
            this);

    public Task<DeviceProperties> GetPropertiesAsync()
    {
        return Connection.CallMethodAsync(CreateGetAllPropertiesMessage(Interface), (m, _) => ReadMessage(m), this);

        static DeviceProperties ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            return ReadProperties(ref reader);
        }
    }

    public ValueTask<IDisposable> WatchPropertiesChangedAsync(
        Action<Exception, PropertyChanges<DeviceProperties>> handler,
        bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return base.WatchPropertiesChangedAsync(
            Interface,
            (m, _) => ReadMessage(m),
            handler,
            emitOnCapturedContext,
            flags);

        static PropertyChanges<DeviceProperties> ReadMessage(Message message)
        {
            var reader = message.GetBodyReader();
            reader.ReadString(); // interface
            HashSet<string> changed = [];
            return new PropertyChanges<DeviceProperties>(
                ReadProperties(ref reader, changed),
                ReadInvalidated(ref reader),
                changed);
        }
    }

    private static DeviceProperties ReadProperties(ref Reader reader, HashSet<string> changedList = null)
    {
        var props = new DeviceProperties();
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
                case "Appearance":
                    reader.ReadSignature("q"u8);
                    props.Appearance = reader.ReadUInt16();
                    break;
                case "Icon":
                    reader.ReadSignature("s"u8);
                    props.Icon = reader.ReadString();
                    break;
                case "Paired":
                    reader.ReadSignature("b"u8);
                    props.Paired = reader.ReadBool();
                    break;
                case "Bonded":
                    reader.ReadSignature("b"u8);
                    props.Bonded = reader.ReadBool();
                    break;
                case "Trusted":
                    reader.ReadSignature("b"u8);
                    props.Trusted = reader.ReadBool();
                    break;
                case "Blocked":
                    reader.ReadSignature("b"u8);
                    props.Blocked = reader.ReadBool();
                    break;
                case "LegacyPairing":
                    reader.ReadSignature("b"u8);
                    props.LegacyPairing = reader.ReadBool();
                    break;
                case "RSSI":
                    reader.ReadSignature("n"u8);
                    props.RSSI = reader.ReadInt16();
                    break;
                case "Connected":
                    reader.ReadSignature("b"u8);
                    props.Connected = reader.ReadBool();
                    break;
                case "UUIDs":
                    reader.ReadSignature("as"u8);
                    props.UUIDs = reader.ReadArrayOfString();
                    break;
                case "Modalias":
                    reader.ReadSignature("s"u8);
                    props.Modalias = reader.ReadString();
                    break;
                case "Adapter":
                    reader.ReadSignature("o"u8);
                    props.Adapter = reader.ReadObjectPath();
                    break;
                case "ManufacturerData":
                    reader.ReadSignature("a{qv}"u8);
                    props.ManufacturerData = ReadType_aeqv(ref reader);
                    break;
                case "ServiceData":
                    reader.ReadSignature("a{sv}"u8);
                    props.ServiceData = reader.ReadDictionaryOfStringToVariantValue();
                    break;
                case "TxPower":
                    reader.ReadSignature("n"u8);
                    props.TxPower = reader.ReadInt16();
                    break;
                case "ServicesResolved":
                    reader.ReadSignature("b"u8);
                    props.ServicesResolved = reader.ReadBool();
                    break;
                case "WakeAllowed":
                    reader.ReadSignature("b"u8);
                    props.WakeAllowed = reader.ReadBool();
                    break;
                default:
                    reader.ReadVariantValue();
                    break;
            }
        }

        return props;
    }
}
