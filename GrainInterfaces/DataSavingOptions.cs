namespace net.jommy.RuuviCore.Interfaces
{
    public class DataSavingOptions
    {
        public int DataSavingInterval { get; set; }
        public bool CalculateAverages { get; set; }
        public bool StoreAcceleration { get; set; }
        public bool DiscardMinMaxValues { get; set; }

        public override string ToString() =>
            $"[Data saving interval: {DataSavingInterval}s, Calculate averages:{CalculateAverages}, Store acceleration:{StoreAcceleration}, Discard packets with min/max values:{DiscardMinMaxValues}]";
    }
}
