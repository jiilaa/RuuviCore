using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface IRuuviTagRegistry : IGrainWithIntegerKey
{
    Task AddOrUpdate(string macAddress, string name);

    ValueTask Refresh(string macAddress, DateTime? timestamp);

    Task<List<RuuviTagInfo>> GetAll();
}