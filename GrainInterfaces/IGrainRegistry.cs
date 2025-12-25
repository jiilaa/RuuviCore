using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface IGrainRegistry<T> : IGrainWithIntegerKey
{
    public const int GrainRegistryId = 0;
        
    Task<List<T>> GetRegisteredGrains();

    Task RegisterInfo(string id, string name, DateTime? lastSeen);
}