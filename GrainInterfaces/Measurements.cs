using System;

namespace net.jommy.RuuviCore.Interfaces
{
    [Serializable]
    public class Measurements
    {
        public DateTime Timestamp { get; set; }

        public decimal Humidity { get; set; }

        public decimal Temperature { get; set; }

        public decimal Pressure { get; set; }

        public int BatteryVoltage { get; set; }

        public Acceleration Acceleration { get; set; }

        public short RSSI { get; set; }

        public int TransmissionPower { get; set; }

        public int MovementCounter { get; set; }

        public int SequenceNumber { get; set; }

        public override string ToString()
        {
            return $"[#{SequenceNumber} @ Timestamp:{Timestamp}. Humidity:{Humidity}, Temperature:{Temperature}, Pressure:{Pressure}, BatteryVoltage:{BatteryVoltage}, Acceleration:{Acceleration}, RSSI:{RSSI}, TX Power:{TransmissionPower}, MovementCounter:{MovementCounter}";
        }
    }
}
