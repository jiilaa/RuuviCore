using System.Runtime.CompilerServices;
using net.jommy.RuuviCore.Bluez.Objects;
using Tmds.DBus.Protocol;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]
namespace net.jommy.RuuviCore.Bluez;

public class BluezObjectFactory
{
    public BluezObjectFactory(Connection connection, string destination)
    {
        (Connection, Destination) = (connection, destination);
    }

    public Connection Connection { get; }
    public string Destination { get; }

    public ObjectManager CreateObjectManager(ObjectPath path)
    {
        return new ObjectManager(this, path);
    }

    public AgentManager CreateAgentManager(ObjectPath path)
    {
        return new AgentManager(this, path);
    }

    public ProfileManager CreateProfileManager(ObjectPath path)
    {
        return new ProfileManager(this, path);
    }

    public HealthManager CreateHealthManager(ObjectPath path)
    {
        return new HealthManager(this, path);
    }

    public Adapter CreateAdapter(ObjectPath path)
    {
        return new Adapter(this, path);
    }

    public GattManager CreateGattManager(ObjectPath path)
    {
        return new GattManager(this, path);
    }

    public Media CreateMedia(ObjectPath path)
    {
        return new Media(this, path);
    }

    public NetworkServer CreateNetworkServer(ObjectPath path)
    {
        return new NetworkServer(this, path);
    }

    public LowEnergyAdvertisingManager CreateLowEnergyAdvertisingManager(ObjectPath path)
    {
        return new LowEnergyAdvertisingManager(this, path);
    }

    public Device CreateDevice(ObjectPath path)
    {
        return new Device(this, path);
    }
}