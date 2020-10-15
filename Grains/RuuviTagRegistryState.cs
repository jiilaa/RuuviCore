using System;
using System.Collections.Generic;
using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains
{
    [Serializable]
    public class RuuviTagRegistryState
    {
        public RuuviTagRegistryState()
        {
            RuuviTags = new Dictionary<string, RuuviTagInfo>();
        }

        public DateTime LastSaved { get; set; }
        public IDictionary<string, RuuviTagInfo> RuuviTags { get; set; }

        [NonSerialized] public bool Dirty;
    }
}