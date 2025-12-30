namespace net.jommy.RuuviCore.Common;

public interface IInfluxSettingsFactory
{
    InfluxSettings GetSettings(string name);
}
