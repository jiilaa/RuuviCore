// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace net.jommy.RuuviCore.Bluez.Models;

public record AdapterProperties
{
    public string Address { get; set; }
    public string AddressType { get; set; }
    public string Name { get; set; }
    public string Alias { get; set; }
    public uint Class { get; set; }
    public bool Powered { get; set; }
    public bool Discoverable { get; set; }
    public uint DiscoverableTimeout { get; set; }
    public bool Pairable { get; set; }
    public uint PairableTimeout { get; set; }
    public bool Discovering { get; set; }
    public IReadOnlyCollection<string> UUIDs { get; set; }
    public string Modalias { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; }
    public IReadOnlyCollection<string> ExperimentalFeatures { get; set; }
}
