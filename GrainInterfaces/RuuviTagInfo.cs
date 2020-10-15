using System;

namespace net.jommy.RuuviCore.Interfaces
{
    public class RuuviTagInfo
    {
        public string MacAddress { get; set; }
        public string Name { get; set; }
        public DateTime ModificationTime { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}