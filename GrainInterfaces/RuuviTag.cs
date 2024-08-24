using System;
using System.Collections.Generic;
using net.jommy.RuuviCore.Common;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("RuuviTag")]
public class RuuviTag
{
    [Orleans.Id(0)]
    public string MacAddress { get; set; }
 
    [Orleans.Id(1)]
    public string Name { get; set; }

    [Orleans.Id(2)]
    public int DataSavingInterval { get; set; }
    
    [Orleans.Id(3)]
    public bool StoreAcceleration { get; set; }
    
    [Orleans.Id(4)]
    public bool StoreName { get; set; }
    
    [Orleans.Id(5)]
    public bool DiscardMinMaxValues { get; set; }
    
    [Orleans.Id(6)]
    public bool AllowMeasurementsThroughGateway { get; set; }
    
    [Orleans.Id(7)]
    public bool IncludeInDashboard { get; set; }
    
    [Orleans.Id(8)]
    public IDictionary<string, AlertThresholds> AlertRules { get; set; }
}