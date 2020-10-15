using System;

namespace net.jommy.RuuviCore.Interfaces
{
    [Serializable]
    public class Acceleration
    {
        public decimal XAxis { get; set; }
        public decimal YAxis { get; set; }
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
}
