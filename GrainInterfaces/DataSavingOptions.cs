using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("net.jommy.RuuviCore.Interfaces.DataSavingOptions")]
public class DataSavingOptions
{
    [Orleans.Id(0)]
    public int DataSavingInterval { get; set; }

    [Orleans.Id(1)]
    public bool CalculateAverages { get; set; }

    [Orleans.Id(2)]
    public bool StoreAcceleration { get; set; }

    [Orleans.Id(3)]
    public bool DiscardMinMaxValues { get; set; }

    [Orleans.Id(4)]
    public TimeSpan BucketSize { get; set; }

    public override string ToString() =>
        $"[Data saving interval: {DataSavingInterval}s, Calculate averages:{CalculateAverages}, Store acceleration:{StoreAcceleration}, Discard packets with min/max values:{DiscardMinMaxValues}]";
}
