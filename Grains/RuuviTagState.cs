using System;
using System.Collections.Generic;
using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains
{
    public class RuuviTagState : IAzureAccessor
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public bool Initialized { get; set; }
        public int DataSavingInterval { get; set; }
        public bool CalculateAverages { get; set; }
        public bool StoreAcceleration { get; set; }
        public bool StoreName { get; set; }
        public bool DiscardMinMaxValues { get; set; }
        public List<Measurements> LatestMeasurements { get; set; }
        public bool UseAzure { get; set; }
        public bool AllowMeasurementsThroughGateway { get; set; }
        public string AzurePrimaryKey { get; set; }
        public string AzureScopeId { get; set; }
        public bool InDashboard { get; set; }

        public IDictionary<string, AlertThresholds> AlertRules { get; set; }

        public RuuviTagState()
        {
            AlertRules = new Dictionary<string, AlertThresholds>();
        }
    }
}
