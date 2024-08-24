using System.Collections.Generic;

namespace net.jommy.RuuviCore;

public class RestApiOptions
{
    public RestApiOptions()
    {
        AllowedDeviceIds = new List<string>();
    }

    public List<string> AllowedDeviceIds { get; set; }
}