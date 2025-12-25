using System;
using System.Buffers.Binary;
using net.jommy.RuuviCore.Interfaces;

// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Grains.DataParsers;

public class RAWv1Parser : RAWParserBase
{
    private const decimal TemperatureMin = -127.99m;
    private const decimal TemperatureMax = 127.99m;
    private const decimal HumidityMin = 0;
    private const decimal HumidityMax = 127.5m;
    private const decimal PressureMin = 500.00m;
    private const decimal PressureMax = 1155.35m;
    private const int BatteryVoltageMin = 0;
    private const int BatteryVoltageMax = 65535;
        
    protected override byte VersionNumber => 3;
    protected override int DataLength => 14;

    public override bool TryParseMeasurements(ReadOnlySpan<byte> data, bool validateValues, out MeasurementDTO measurements)
    {
        EnsureVersionAndLength(data);

        measurements = new MeasurementDTO
        {
            Humidity = data[1] * 0.5m,
            Temperature = GetTemperature(data.Slice(2)),
            Pressure = (BinaryPrimitives.ReadUInt16BigEndian(data.Slice(4)) + 50000) / 100m,
            BatteryVoltage = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(12)),
            Acceleration = ParseAcceleration(data.Slice(6, 6))
        };

        if (validateValues)
        {
            if (measurements.Temperature == TemperatureMin || measurements.Temperature == TemperatureMax
                                                           || measurements.Humidity == HumidityMin || measurements.Humidity == HumidityMax
                                                           || measurements.Pressure == PressureMin || measurements.Pressure == PressureMax
                                                           || measurements.BatteryVoltage == BatteryVoltageMin || measurements.BatteryVoltage == BatteryVoltageMax)
            {
                measurements = null;
                return false;
            }
        }

        return true;
    }

    private static decimal GetTemperature(ReadOnlySpan<byte> data)
    {
        decimal temp = data[0] & 0b1111111;
        temp += data[1] / 100m;
        var sign = (data[0] >> 7) & 1;

        return sign == 0 ? Math.Round(temp, 2) : Math.Round(-1 * temp, 2);
    }
}