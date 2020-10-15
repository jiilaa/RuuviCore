using System;
using System.Buffers.Binary;
using net.jommy.RuuviCore.Interfaces;

// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Grains.DataParsers
{
    public class RAWv2Parser : RAWParserBase
    {
        private const decimal TemperatureMin = -163.835m;
        private const decimal TemperatureMax = 163.835m;
        private const decimal HumidityMin = 0;
        private const decimal HumidityMax = 163.835m;
        private const int PressureMin = 50000;
        private const int PressureMax = 115534;
        private const int BatteryVoltageMin = 1600;
        private const int BatteryVoltageMax = 3646;
        private const int TxPowerMin = -40;
        private const int TxPowerMax = 20;
        
        protected override byte VersionNumber => 5;
        protected override int DataLength => 24;

        public override bool TryParseMeasurements(ReadOnlySpan<byte> data, bool validateValues, out Measurements measurements)
        {
            EnsureVersionAndLength(data);

            measurements = new Measurements
            {
                Temperature = TwosComplement(data.Slice(1), 16) * 0.005m,
                Humidity = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(3)) * 0.0025m,
                Pressure =  (BinaryPrimitives.ReadUInt16BigEndian(data.Slice(5)) + 50000) / 100m,
                Acceleration = ParseAcceleration(data.Slice(7)),
                BatteryVoltage = ParseBatteryVoltage(data.Slice(13)),
                TransmissionPower = (data[14] & 0b11111) * 2 - 40,
                MovementCounter = data[15],
                SequenceNumber = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(16)),
            };

            if (validateValues)
            {
                if (
                    measurements.Temperature == TemperatureMin || measurements.Temperature == TemperatureMax
                    || measurements.Humidity == HumidityMin || measurements.Humidity == HumidityMax
                    || measurements.Pressure == PressureMin || measurements.Pressure == PressureMax
                    || measurements.BatteryVoltage == BatteryVoltageMin || measurements.BatteryVoltage == BatteryVoltageMax
                    || measurements.TransmissionPower == TxPowerMin || measurements.TransmissionPower == TxPowerMax)
                {
                    measurements = null;
                    return false;
                }
            }

            return true;
        }

        private static int ParseBatteryVoltage(ReadOnlySpan<byte> data)
        {
            return (BinaryPrimitives.ReadUInt16BigEndian(data) >> 5) + 1600;
        }
    }
}
