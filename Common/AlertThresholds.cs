using System;

namespace net.jommy.RuuviCore.Common;

[Serializable]
[Orleans.GenerateSerializer]
public class AlertThresholds
{
    [Orleans.Id(0)]
    public decimal? MinValidValue { get; set; }

    [Orleans.Id(1)]
    public decimal? MaxValidValue { get; set; }
}