using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.Bluez.Objects;

public class ObjectManager : BluezObject
{
    private const string Interface = "org.freedesktop.DBus.ObjectManager";

    public ObjectManager(BluezObjectFactory objectFactory, ObjectPath path) : base(objectFactory, path)
    {
    }

    public Task<Dictionary<ObjectPath, Dictionary<string, Dictionary<string, VariantValue>>>> GetManagedObjectsAsync()
    {
        return Connection.CallMethodAsync(CreateMessage(), (m, s) => ReadMessage_aeoaesaesv(m), this);

        MessageBuffer CreateMessage()
        {
            var writer = Connection.GetMessageWriter();
            writer.WriteMethodCallHeader(
                ObjectFactory.Destination,
                Path,
                Interface,
                "GetManagedObjects");
            return writer.CreateMessage();
        }
    }

    public ValueTask<IDisposable> WatchInterfacesAddedAsync(
        Action<Exception, (ObjectPath Object, Dictionary<string, Dictionary<string, VariantValue>> Interfaces)> handler,
        bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return WatchSignalAsync(
            ObjectFactory.Destination,
            Interface,
            Path,
            "InterfacesAdded",
            (m, s) => ReadMessage_oaesaesv(m),
            handler,
            emitOnCapturedContext,
            flags);
    }

    public ValueTask<IDisposable> WatchInterfacesRemovedAsync(
        Action<Exception, (ObjectPath Object, string[] Interfaces)> handler,
        bool emitOnCapturedContext = true,
        ObserverFlags flags = ObserverFlags.None)
    {
        return WatchSignalAsync(
            ObjectFactory.Destination,
            Interface,
            Path,
            "InterfacesRemoved",
            (m, s) => ReadMessage_oas(m),
            handler,
            emitOnCapturedContext,
            flags);
    }
}
