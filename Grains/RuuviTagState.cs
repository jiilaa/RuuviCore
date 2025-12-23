using System;
using System.Collections.Generic;

using net.jommy.RuuviCore.Common;
using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains;

public class RuuviTagState
{
    public string MacAddress { get; set; }
    public string Name { get; set; }
    public bool Initialized { get; set; }
    public int DataSavingInterval { get; set; }
    public bool CalculateAverages { get; set; }
    public bool StoreAcceleration { get; set; }
    public bool StoreName { get; set; }
    public bool DiscardMinMaxValues { get; set; }

    public DateTime? CurrentBucketStartTime { get; set; }

    public List<MeasurementDTO> CurrentBucketMeasurements { get; set; } = [];

    public List<CachedMeasurement> CachedMeasurements { get; set; } = [];

    public bool AllowMeasurementsThroughGateway { get; set; }
    public bool InDashboard { get; set; }

    public short SignalStrength { get; set; }

    public TimeSpan BucketSize { get; set; }

    public IDictionary<string, AlertThresholds> AlertRules { get; set; } = new Dictionary<string, AlertThresholds>();
}
