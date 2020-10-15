using System;

namespace net.jommy.RuuviCore.Common
{
    public static class GrainExtensions
    {
        public static Guid ToActorGuid(this string macAddress)
        {
            return Orleans.Streams.Utils.StreamProviderUtils.GenerateStreamGuid(macAddress);
        }
    }
}