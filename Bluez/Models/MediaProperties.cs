// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace net.jommy.RuuviCore.Bluez.Models;

public record MediaProperties
{
    public IReadOnlyCollection<string> SupportedUUIDs { get; set; }
}
