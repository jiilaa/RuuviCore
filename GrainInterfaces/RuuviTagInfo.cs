using System;

namespace net.jommy.RuuviCore.Interfaces;

[Serializable]
[Orleans.GenerateSerializer]
[Orleans.Alias("RuuviTagInfo")]
public class RuuviTagInfo
{
    [Orleans.Id(0)]
    public string MacAddress { get; set; }
    
    [Orleans.Id(1)]
    public string Name { get; set; }
    
    [Orleans.Id(2)]
    public DateTime ModificationTime { get; set; }
    
    [Orleans.Id(3)]
    public DateTime? LastSeen { get; set; }
}