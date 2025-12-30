using System;
using System.Buffers.Binary;
using net.jommy.RuuviCore.Interfaces;

// ReSharper disable InconsistentNaming

namespace net.jommy.RuuviCore.Grains.DataParsers;

public class RuuviProtocol6Parser : RAWParserBase
{
    private const decimal TemperatureMin = -163.835m;
    private const decimal TemperatureMax = 163.835m;
    private const decimal HumidityMin = 0;
    private const decimal HumidityMax = 100m;
    private const int PressureMin = 50000;
    private const int PressureMax = 115534;
    private const decimal PM25Min = 0;
    private const decimal PM25Max = 1000m;
    private const int CO2Min = 0;
    private const int CO2Max = 40000;
    private const int VOCMin = 0;
    private const int VOCMax = 500;
    private const int NOXMin = 0;
    private const int NOXMax = 500;

    // Luminosity encoding constant: DELTA = ln(65536) / 254
    private const double LuminosityDelta = 0.04448084;

    protected override byte VersionNumber => 6;
    protected override int DataLength => 20;

    public override bool TryParseMeasurements(ReadOnlySpan<byte> data, bool validateValues, out MeasurementDTO measurements)
    {
        EnsureVersionAndLength(data);

        var temperature = TwosComplement(data[1..], 16) * 0.005m;
        var humidity = BinaryPrimitives.ReadUInt16BigEndian(data[3..]) * 0.0025m;
        var pressure = (BinaryPrimitives.ReadUInt16BigEndian(data[5..]) + 50000) / 100m;
        var pm25 = BinaryPrimitives.ReadUInt16BigEndian(data[7..]) * 0.1m;
        var co2 = BinaryPrimitives.ReadUInt16BigEndian(data[9..]);

        // VOC and NOX are 9-bit values split across bytes
        var vocMsb = data[11];
        var noxMsb = data[12];
        var flags = data[16];
        var vocLsb = (flags >> 6) & 0x1;
        var noxLsb = (flags >> 7) & 0x1;
        var voc = (vocMsb << 1) | vocLsb;
        var nox = (noxMsb << 1) | noxLsb;

        var luminosityCode = data[13];
        var sequence = data[15];

        measurements = new MeasurementDTO
        {
            Temperature = temperature,
            Humidity = humidity,
            Pressure = pressure,
            SequenceNumber = sequence
        };

        // Check for invalid values (all bits set = sensor unavailable)
        if (validateValues)
        {
            // Check if all measurement fields are at invalid sentinel values
            var allInvalid =
                BinaryPrimitives.ReadUInt16BigEndian(data.Slice(1)) == 0x8000 &&  // Temperature invalid
                BinaryPrimitives.ReadUInt16BigEndian(data.Slice(3)) == 0xFFFF &&  // Humidity invalid
                BinaryPrimitives.ReadUInt16BigEndian(data.Slice(5)) == 0xFFFF &&  // Pressure invalid
                BinaryPrimitives.ReadUInt16BigEndian(data.Slice(7)) == 0xFFFF &&  // PM2.5 invalid
                BinaryPrimitives.ReadUInt16BigEndian(data.Slice(9)) == 0xFFFF &&  // CO2 invalid
                data[11] == 0xFF &&  // VOC invalid
                data[12] == 0xFF;    // NOX invalid

            if (allInvalid)
            {
                measurements = null;
                return false;
            }

            // Check for extreme values that indicate invalid readings
            if (temperature == TemperatureMin || temperature == TemperatureMax ||
                humidity == HumidityMin || humidity == HumidityMax ||
                pressure < PressureMin / 100m || pressure > PressureMax / 100m)
            {
                measurements = null;
                return false;
            }
        }

        // Parse air quality data (only if not invalid)
        if (pm25 != PM25Max || co2 != CO2Max || voc != VOCMax || nox != NOXMax)
        {
            measurements.AirQuality = new AirQuality
            {
                ParticulateMatter25 = pm25 == PM25Max ? null : pm25,
                CO2Concentration = co2 == CO2Max ? null : co2,
                VolatileOrganicCompoundsIndex = voc == VOCMax ? null : voc,
                NitrogenOxidesIndex = nox == NOXMax ? null : nox
            };
        }

        // Parse luminosity (logarithmic decoding)
        if (luminosityCode is > 0 and < 254)
        {
            measurements.Luminosity = DecodeLuminosity(luminosityCode);
        }
        else if (luminosityCode == 254)
        {
            measurements.Luminosity = 65535m;  // Maximum value
        }

        return true;
    }

    private static decimal DecodeLuminosity(byte code)
    {
        // Inverse of: CODE = round(ln(value + 1) / DELTA)
        // Therefore: value = exp(CODE * DELTA) - 1
        var value = Math.Exp(code * LuminosityDelta) - 1;
        return (decimal)Math.Round(value, 2);
    }
}
