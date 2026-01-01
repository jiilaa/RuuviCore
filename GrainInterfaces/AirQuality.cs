using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("AirQuality")]
public class AirQuality
{
    [Orleans.Id(0)]
    public int? VolatileOrganicCompoundsIndex { get; set; }

    [Orleans.Id(1)]
    public int? NitrogenOxidesIndex { get; set; }

    [Orleans.Id(2)]
    public int? CO2Concentration { get; set; }

    [Orleans.Id(3)]
    public decimal? ParticulateMatter25 { get; set; }

    public override string ToString()
    {
        return $"VOX:{VolatileOrganicCompoundsIndex}, NOX:{NitrogenOxidesIndex}, CO2:{CO2Concentration}, PM2.5:{ParticulateMatter25}";
    }
}
