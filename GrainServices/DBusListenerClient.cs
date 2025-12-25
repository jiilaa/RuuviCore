using System;
using System.Threading.Tasks;
using net.jommy.RuuviCore.Interfaces;
using Orleans.Runtime.Services;

namespace net.jommy.RuuviCore.GrainServices;

public class DBusListenerClient : GrainServiceClient<IRuuviDBusListener>, IDBusListenerClient
{
    public DBusListenerClient(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }

    private IRuuviDBusListener GrainService => GetGrainService(CurrentGrainReference.GrainId);

    public Task SimulateEvent(string macAddress) => GrainService.SimulateEvent(macAddress);
}