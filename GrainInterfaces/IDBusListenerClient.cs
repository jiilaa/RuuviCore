using Orleans.Services;

namespace net.jommy.RuuviCore.Interfaces;

public interface IDBusListenerClient : IGrainServiceClient<IRuuviDBusListener>, IRuuviDBusListener
{
}