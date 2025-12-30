using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("MeasurementDTO")]
public class MeasurementDTO
{
    [Orleans.Id(0)]
    public DateTime Timestamp { get; set; }

    [Orleans.Id(1)]
    public decimal Humidity { get; set; }

    [Orleans.Id(2)]
    public decimal Temperature { get; set; }

    [Orleans.Id(3)]
    public decimal Pressure { get; set; }

    [Orleans.Id(4)]
    public int BatteryVoltage { get; set; }

    [Orleans.Id(5)]
    public Acceleration Acceleration { get; set; }

    [Orleans.Id(6)]
    public short RSSI { get; set; }

    [Orleans.Id(7)]
    public int TransmissionPower { get; set; }

    [Orleans.Id(8)]
    public int MovementCounter { get; set; }

    [Orleans.Id(9)]
    public int SequenceNumber { get; set; }

    [Orleans.Id(10)]
    public AirQuality AirQuality { get; set; }

    [Orleans.Id(11)]
    public decimal? Luminosity { get; set; }

    public override string ToString()
    {
        if (Acceleration != null)
        {
            return $"[#{SequenceNumber} @ {Timestamp}. Humidity:{Humidity}, Temperature:{Temperature}, Pressure:{Pressure}, Battery:{BatteryVoltage}, Acceleration:{Acceleration}, RSSI:{RSSI}, Movement:{MovementCounter}";
        }
        return $"[#{SequenceNumber} @ {Timestamp}. Humidity:{Humidity}, Temperature:{Temperature}, Pressure:{Pressure}, Battery:{BatteryVoltage}, RSSI:{RSSI}, Movement:{MovementCounter}";
    }
}
