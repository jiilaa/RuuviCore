using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("MeasurementEnvelope")]
public class MeasurementEnvelope
{
    [Orleans.Id(0)]
    public DateTime Timestamp { get; set; }
 
    [Orleans.Id(1)]
    public short? SignalStrength { get; set; }

    [Orleans.Id(2)]
    public byte[] Data { get; set; }
}