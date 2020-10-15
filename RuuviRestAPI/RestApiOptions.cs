using System.Collections.Generic;

namespace RuuviRestAPI
{
    public class RestApiOptions
    {
        public RestApiOptions()
        {
            AllowedDeviceIds = new List<string>();
        }

        public List<string> AllowedDeviceIds { get; set; }
    }
}