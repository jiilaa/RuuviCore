using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains;

public class CachedMeasurement
{
    public bool Sent { get; set; }
    public MeasurementDTO Measurement { get; set; }
}
