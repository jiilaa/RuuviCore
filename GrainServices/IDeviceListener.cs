using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tmds.DBus.Protocol;

namespace net.jommy.RuuviCore.GrainServices;

public interface IDeviceListener : IDisposable
{
    Task StartListeningAsync();

    bool IsAlive();

    Task HandleDataAsync(Dictionary<ushort, VariantValue> manufacturerData);
}