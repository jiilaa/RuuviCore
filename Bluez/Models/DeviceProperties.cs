using System.Collections.Generic;
using Tmds.DBus.Protocol;
// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Bluez.Models;

public record DeviceProperties
{
    public string Address { get; set; }
    public string AddressType { get; set; }
    public string Name { get; set; }
    public string Alias { get; set; }
    public uint Class { get; set; }
    public ushort Appearance { get; set; }
    public string Icon { get; set; }
    public bool Paired { get; set; }
    public bool Bonded { get; set; }
    public bool Trusted { get; set; }
    public bool Blocked { get; set; }
    public bool LegacyPairing { get; set; }
    public short RSSI { get; set; }
    public bool Connected { get; set; }
    public IReadOnlyCollection<string> UUIDs { get; set; }
    public string Modalias { get; set; }
    public ObjectPath Adapter { get; set; }
    public Dictionary<ushort, VariantValue> ManufacturerData { get; set; }
    public Dictionary<string, VariantValue> ServiceData { get; set; }
    public short TxPower { get; set; }
    public bool ServicesResolved { get; set; }
    public bool WakeAllowed { get; set; }
}
