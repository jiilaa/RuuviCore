using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains;

public class CachedMeasurement
{
    public CachedMeasurement()
    {
        Sent = false;
    }

    public CachedMeasurement(MeasurementDTO measurement)
    {
        Measurement = measurement;
        Sent = false;
    }

    public bool Sent { get; set; }

    public MeasurementDTO Measurement { get; init; }
}
