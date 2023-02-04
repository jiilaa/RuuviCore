﻿using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface IInfluxBridge : IGrainWithStringKey
{
    Task<bool> WriteMeasurements(string macAddress, string name, Measurements measurements);

    Task<bool> IsValid();
}