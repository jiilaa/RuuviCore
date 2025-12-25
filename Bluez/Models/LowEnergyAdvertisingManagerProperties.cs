using System.Collections.Generic;

namespace net.jommy.RuuviCore.Bluez.Models;

public record LowEnergyAdvertisingManagerProperties
{
    public byte ActiveInstances { get; set; }
    public byte SupportedInstances { get; set; }
    public IReadOnlyCollection<string> SupportedIncludes { get; set; }
    public IReadOnlyCollection<string> SupportedSecondaryChannels { get; set; }
}
