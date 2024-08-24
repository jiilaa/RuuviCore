namespace net.jommy.RuuviCore.Grains;

public interface IAzureAccessor
{
    public string AzurePrimaryKey { get; }
    public string AzureScopeId { get; }
}