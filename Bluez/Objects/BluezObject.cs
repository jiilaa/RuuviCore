using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using net.jommy.RuuviCore.Bluez.Models;

using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

/// <summary>
/// A base class for all BlueZ D-Bus objects.
/// Contains common functionality for interacting with D-Bus properties and signals.
/// Methods have weird names like ReadMessage_aeoaesaesv because they correspond to D-Bus type signatures.
/// D-Bus Type Signature Codes:
///     | Code | Type        | Example             |
///     |------|-------------|---------------------|
///     | v    | variant     | any type            |
///     | a    | array       | list/collection     |
///     | s    | string      | text                |
///     | e    | entry       | dict key-value pair |
///     | q    | uint16      | unsigned 16-bit int |
///     | o    | object path | D-Bus object path   |
///     | u    | uint32      | unsigned 32-bit int |
///     | b    | boolean     | true/false          |
///     | y    | byte        | 8-bit value         |
///     | n    | int16       | signed 16-bit int   |
/// </summary>


public abstract class BluezObject
{
    protected BluezObject(BluezObjectFactory objectFactory, ObjectPath path)
    {
        (ObjectFactory, Path) = (objectFactory, path);
    }

    protected BluezObjectFactory ObjectFactory { get; }
    public ObjectPath Path { get; }
    protected Connection Connection => ObjectFactory.Connection;

    protected MessageBuffer CreateGetPropertyMessage(string @interface, string property)
    {
        var writer = Connection.GetMessageWriter();
        writer.WriteMethodCallHeader(
            ObjectFactory.Destination,
            Path,
            "org.freedesktop.DBus.Properties",
            signature: "ss",
            member: "Get");
        writer.WriteString(@interface);
        writer.WriteString(property);
        return writer.CreateMessage();
    }

    protected MessageBuffer CreateGetAllPropertiesMessage(string @interface)
    {
        var writer = Connection.GetMessageWriter();
        writer.WriteMethodCallHeader(
            ObjectFactory.Destination,
            Path,
            "org.freedesktop.DBus.Properties",
            signature: "s",
            member: "GetAll");
        writer.WriteString(@interface);
        return writer.CreateMessage();
    }

    protected ValueTask<IDisposable> WatchPropertiesChangedAsync<TProperties>(
        string @interface,
        MessageValueReader<PropertyChanges<TProperties>> reader,
        Action<Exception, PropertyChanges<TProperties>> handler,
        bool emitOnCapturedContext,
        ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = ObjectFactory.Destination,
            Path = Path,
            Interface = "org.freedesktop.DBus.Properties",
            Member = "PropertiesChanged",
            Arg0 = @interface
        };
        return Connection.AddMatchAsync(
            rule,
            reader,
            (ex, changes, _, hs) => ((Action<Exception, PropertyChanges<TProperties>>)hs!).Invoke(ex, changes),
            this,
            handler,
            emitOnCapturedContext,
            flags);
    }

    protected ValueTask<IDisposable> WatchSignalAsync<TArg>(
        string sender,
        string @interface,
        ObjectPath path,
        string signal,
        MessageValueReader<TArg> reader,
        Action<Exception, TArg> handler,
        bool emitOnCapturedContext,
        ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = sender,
            Path = path,
            Member = signal,
            Interface = @interface
        };
        return Connection.AddMatchAsync(
            rule,
            reader,
            (ex, arg, _, hs) => ((Action<Exception, TArg>)hs!).Invoke(ex, arg),
            this,
            handler,
            emitOnCapturedContext,
            flags);
    }

    public ValueTask<IDisposable> WatchSignalAsync(
        string sender,
        string @interface,
        ObjectPath path,
        string signal,
        Action<Exception> handler,
        bool emitOnCapturedContext,
        ObserverFlags flags)
    {
        var rule = new MatchRule
        {
            Type = MessageType.Signal,
            Sender = sender,
            Path = path,
            Member = signal,
            Interface = @interface
        };
        return Connection.AddMatchAsync<object>(
            rule,
            (_, _) => null!,
            (ex, _, _, hs) => ((Action<Exception>)hs!).Invoke(ex),
            this,
            handler,
            emitOnCapturedContext,
            flags);
    }

    protected static Dictionary<ObjectPath, Dictionary<string, Dictionary<string, VariantValue>>>
        ReadMessage_aeoaesaesv(Message message)
    {
        var reader = message.GetBodyReader();
        return ReadType_aeoaesaesv(ref reader);
    }

    protected static (ObjectPath, Dictionary<string, Dictionary<string, VariantValue>>) ReadMessage_oaesaesv(
        Message message)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadObjectPath();
        var arg1 = ReadType_aesaesv(ref reader);
        return (arg0, arg1);
    }

    protected static (ObjectPath, string[]) ReadMessage_oas(Message message)
    {
        var reader = message.GetBodyReader();
        var arg0 = reader.ReadObjectPath();
        var arg1 = reader.ReadArrayOfString();
        return (arg0, arg1);
    }

    protected static ObjectPath ReadMessage_o(Message message)
    {
        var reader = message.GetBodyReader();
        return reader.ReadObjectPath();
    }

    protected static string[] ReadMessage_as(Message message)
    {
        var reader = message.GetBodyReader();
        return reader.ReadArrayOfString();
    }

    protected static string ReadMessage_v_s(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("s"u8);
        return reader.ReadString();
    }

    protected static uint ReadMessage_v_u(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("u"u8);
        return reader.ReadUInt32();
    }

    protected static bool ReadMessage_v_b(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("b"u8);
        return reader.ReadBool();
    }

    protected static IReadOnlyCollection<string> ReadMessage_v_as(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("as"u8);
        return reader.ReadArrayOfString();
    }

    protected static byte ReadMessage_v_y(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("y"u8);
        return reader.ReadByte();
    }

    protected static short ReadMessage_v_n(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("n"u8);
        return reader.ReadInt16();
    }

    protected static ObjectPath ReadMessage_v_o(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("o"u8);
        return reader.ReadObjectPath();
    }

    protected static ushort ReadMessage_v_q(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("q"u8);
        return reader.ReadUInt16();
    }

    protected static Dictionary<string, VariantValue> ReadMessage_v_aesv(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("a{sv}"u8);
        return reader.ReadDictionaryOfStringToVariantValue();
    }

    protected static Dictionary<ushort, VariantValue> ReadMessage_v_aeqv(Message message)
    {
        var reader = message.GetBodyReader();
        reader.ReadSignature("a{qv}"u8);
        return ReadType_aeqv(ref reader);
    }

    protected static Dictionary<ushort, VariantValue> ReadType_aeqv(ref Reader reader)
    {
        Dictionary<ushort, VariantValue> dictionary = new();
        var dictEnd = reader.ReadDictionaryStart();
        while (reader.HasNext(dictEnd))
        {
            var key = reader.ReadUInt16();
            var value = reader.ReadVariantValue();
            dictionary[key] = value;
        }

        return dictionary;
    }

    protected static HashSet<string> ReadInvalidated(ref Reader reader)
    {
        HashSet<string> invalidated = [];
        var arrayEnd = reader.ReadArrayStart(DBusType.String);
        while (reader.HasNext(arrayEnd))
        {
            var property = reader.ReadString();
            invalidated.Add(property);
        }

        return invalidated;
    }

    private static Dictionary<ObjectPath, Dictionary<string, Dictionary<string, VariantValue>>> ReadType_aeoaesaesv(
        ref Reader reader)
    {
        var dictionary = new Dictionary<ObjectPath, Dictionary<string, Dictionary<string, VariantValue>>>();
        var dictEnd = reader.ReadDictionaryStart();
        while (reader.HasNext(dictEnd))
        {
            var key = reader.ReadObjectPath();
            var value = ReadType_aesaesv(ref reader);
            dictionary[key] = value;
        }

        return dictionary;
    }

    private static Dictionary<string, Dictionary<string, VariantValue>> ReadType_aesaesv(ref Reader reader)
    {
        Dictionary<string, Dictionary<string, VariantValue>> dictionary = new();
        var dictEnd = reader.ReadDictionaryStart();
        while (reader.HasNext(dictEnd))
        {
            var key = reader.ReadString();
            var value = reader.ReadDictionaryOfStringToVariantValue();
            dictionary[key] = value;
        }

        return dictionary;
    }
}
