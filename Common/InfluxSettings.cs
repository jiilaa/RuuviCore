namespace net.jommy.RuuviCore.Common
{
    public class InfluxSettings
    {
        private const string DefaultInfluxAddress = "http://localhost:8086";
        private const string DefaultInfluxDatabase = "ruuvidata";
        private const string DefaultInfluxMeasurementTable = "measurements";

        public string BridgeName { get; set; }
        public string InfluxAddress { get; set; }
        public string InfluxDatabase { get; set; }
        public string InfluxMeasurementTable { get; set; }

        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public InfluxSettings()
        {
            InfluxAddress = DefaultInfluxAddress;
            InfluxDatabase = DefaultInfluxDatabase;
            InfluxMeasurementTable = DefaultInfluxMeasurementTable;
        }
    }
}