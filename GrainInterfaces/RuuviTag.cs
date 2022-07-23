using System;
using System.Collections.Generic;
using net.jommy.RuuviCore.Common;

namespace net.jommy.RuuviCore.Interfaces
{
    [Serializable]
    public class RuuviTag
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public int DataSavingInterval { get; set; }
        public bool StoreAcceleration { get; set; }
        public bool StoreName { get; set; }
        public bool DiscardMinMaxValues { get; set; }
        public bool AllowMeasurementsThroughGateway { get; set; }
        public bool IncludeInDashboard { get; set; }
        public IDictionary<string, AlertThresholds> AlertRules { get; set; }
    }
}