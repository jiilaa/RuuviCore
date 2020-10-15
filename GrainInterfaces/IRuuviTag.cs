using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;

namespace net.jommy.RuuviCore.Interfaces
{
    public interface IRuuviTag : IGrainWithGuidKey
    {
        Task Initialize(string macAddress, string name, DataSavingOptions dataSavingOptions);
        
        Task SetName(string name);

        Task<string> GetName();

        Task SetDataSavingOptions(DataSavingOptions options);

        Task<DataSavingOptions> GetDataSavingOptions();

        Task StoreMeasurementData(Measurements measurements);

        Task<List<Measurements>> GetCachedMeasurements();

        Task UseAzure(AzureState state, string scopeId, string primaryKey);

        Task<bool> MeasurementsAllowedThroughGateway();

        Task AllowMeasurementsThroughGateway(bool allowed);
        
        Task<RuuviTag> GetTag();

        Task Edit(RuuviTag ruuviTag);
    }
}
