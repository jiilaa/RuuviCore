using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
public class MeasurementEnvelope
{
    public string MacAddress { get; set; }
    public DateTime Timestamp { get; set; }
    public short? SignalStrength { get; set; }
    public byte[] Data { get; set; }
}