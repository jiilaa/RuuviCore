using System.Collections.Generic;

namespace net.jommy.RuuviCore.Common;

public class DBusSettings
{
    private const string DefaultAdapterName = "hci0";
    public const string SimulatedAdapterName = "simulated";
        
    public string BluetoothAdapterName { get; set; }

    public List<string> SimulatedDevices { get; set; } = [];

    public DBusSettings()
    {
        BluetoothAdapterName = DefaultAdapterName;
    }
}