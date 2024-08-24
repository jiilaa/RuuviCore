namespace net.jommy.RuuviCore.Common;

public class DBusSettings
{
    private const string DefaultAdapterName = "hci0";
        
    public string BluetoothAdapterName { get; set; }

    public bool Simulate { get; set; }

    public DBusSettings()
    {
        BluetoothAdapterName = DefaultAdapterName;
    }
}