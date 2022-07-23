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
            if (Acceleration != null)
            {
                return $"[#{SequenceNumber} @ {Timestamp}. Humidity:{Humidity}, Temperature:{Temperature}, Pressure:{Pressure}, Battery:{BatteryVoltage}, Acceleration:{Acceleration}, RSSI:{RSSI}, Movement:{MovementCounter}";
            }
            return $"[#{SequenceNumber} @ {Timestamp}. Humidity:{Humidity}, Temperature:{Temperature}, Pressure:{Pressure}, Battery:{BatteryVoltage}, RSSI:{RSSI}, Movement:{MovementCounter}";
        }
    }
}
