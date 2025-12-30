using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces;

public interface IRuuviTag : IGrainWithStringKey
{
    Task Initialize(string macAddress, string name, DataSavingOptions dataSavingOptions, List<string> bridges);

    Task SetName(string name);

    Task SetBridges(List<string> bridges);

    Task<string> GetName();

    Task SetDataSavingOptions(DataSavingOptions options);

    Task<DataSavingOptions> GetDataSavingOptions();

    Task StoreMeasurementData(MeasurementDTO measurements);

    Task ReceiveMeasurements(MeasurementEnvelope measurementEnvelope);

    Task<List<MeasurementDTO>> GetCachedMeasurements();

    Task<bool> MeasurementsAllowedThroughGateway();

    Task AllowMeasurementsThroughGateway(bool allowed);
        
    Task<RuuviTag> GetTag();

    Task Edit(RuuviTag ruuviTag);
}