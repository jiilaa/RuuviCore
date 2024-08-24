using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("Acceleration")]
public class Acceleration
{
    [Orleans.Id(0)]
    public decimal XAxis { get; set; }
    [Orleans.Id(1)]
    public decimal YAxis { get; set; }
    [Orleans.Id(2)]
    public decimal ZAxis { get; set; }

    public Acceleration()
    {
    }

    public Acceleration(decimal xAxis, decimal yAxis, decimal zAxis)
    {
        XAxis = xAxis;
        YAxis = yAxis;
        ZAxis = zAxis;
    }

    public override string ToString()
    {
        return $"X:{XAxis}, Y:{YAxis}, Z:{ZAxis}";
    }
}