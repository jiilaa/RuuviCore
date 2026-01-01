using System;
using System.IO;
using AwesomeAssertions;
using net.jommy.RuuviCore.Interfaces;
using net.jommy.RuuviCore.Protocol.DataParsers;

using NUnit.Framework;

namespace UnitTests.DataParserTests;

[TestFixture]
public class RuuviProtocol6ParserTests
{
    private RuuviProtocol6Parser _parser;

    [SetUp]
    public void SetUp()
    {
        _parser = new RuuviProtocol6Parser();
    }

    [Test]
    public void Parser_WithInvalidDataLength_ThrowsInvalidDataException()
    {
        var invalidData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        Action action = () => _parser.TryParseMeasurements(invalidData, false, out _);

        action.Should().Throw<InvalidDataException>("because data length is invalid.");
    }

    [Test]
    public void Parser_WithInvalidDataVersion_ThrowsInvalidDataException()
    {
        var invalidData = new byte[] { 0x05, 0x17, 0x0C, 0x56, 0x68, 0xC7, 0x9E, 0x00, 0x70, 0x00, 0xC9, 0x05, 0x01, 0xD9, 0x00, 0xCD, 0x00, 0x4C, 0x88, 0x4F };
        Action action = () => _parser.TryParseMeasurements(invalidData, false, out _);

        action.Should().Throw<InvalidDataException>("because data version is invalid.");
    }

    [Test]
    public void Parser_WithValidData_ReturnsCorrectMeasurements()
    {
        // Test vector from https://docs.ruuvi.com/communication/bluetooth-advertisements/data-format-6
        // Temperature: 29.500°C, Humidity: 55.300%, Pressure: 101,102 Pa (1011.02 hPa)
        // PM 2.5: 11.2 µg/m³, CO2: 201 ppm, VOC: 10, NOX: 2
        // Luminosity: 13,026.67 Lux, Sequence: 205
        var validData = new byte[] { 0x06, 0x17, 0x0C, 0x56, 0x68, 0xC7, 0x9E, 0x00, 0x70, 0x00, 0xC9, 0x05, 0x01, 0xD9, 0x00, 0xCD, 0x00, 0x4C, 0x88, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Temperature = 29.500m,
            Humidity = 55.300m,
            Pressure = 1011.02m,
            SequenceNumber = 205,
            AirQuality = new AirQuality
            {
                ParticulateMatter25 = 11.2m,
                CO2Concentration = 201,
                VolatileOrganicCompoundsIndex = 10,
                NitrogenOxidesIndex = 2
            },
            Luminosity = 13026.67m
        };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
        valid.Should().BeTrue();
    }

    [Test]
    public void Parser_WithMaximumValues_ReturnsCorrectMeasurements()
    {
        // Test vector for maximum values (with calibration in progress)
        var validData = new byte[] { 0x06, 0x7F, 0xFF, 0x9C, 0x40, 0xFF, 0xFE, 0x27, 0x10, 0x9C, 0x40, 0xFA, 0xFA, 0xFE, 0x00, 0xFF, 0x07, 0x4C, 0x8F, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Temperature = 163.835m,
            Humidity = 100.000m,
            Pressure = 1155.34m,
            SequenceNumber = 255,
            AirQuality = new AirQuality
            {
                ParticulateMatter25 = 1000.0m,
                CO2Concentration = 40000,
                VolatileOrganicCompoundsIndex = 500,
                NitrogenOxidesIndex = 500
            },
            Luminosity = 65535m
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }

    [Test]
    public void Parser_WithMaximumValuesAndValidationOn_ReturnsFalse()
    {
        // Maximum values should be treated as invalid when validation is on
        var validData = new byte[] { 0x06, 0x7F, 0xFF, 0x9C, 0x40, 0xFF, 0xFE, 0x27, 0x10, 0x9C, 0x40, 0xFA, 0xFA, 0xFE, 0x00, 0xFF, 0x07, 0x4C, 0x8F, 0x4F };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }

    [Test]
    public void Parser_WithMinimumValues_ReturnsCorrectMeasurements()
    {
        // Test vector for minimum values
        var validData = new byte[] { 0x06, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x88, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Temperature = -163.835m,
            Humidity = 0m,
            Pressure = 500.00m,
            SequenceNumber = 0,
            AirQuality = new AirQuality
            {
                ParticulateMatter25 = 0m,
                CO2Concentration = 0,
                VolatileOrganicCompoundsIndex = 0,
                NitrogenOxidesIndex = 0
            },
            Luminosity = null
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }

    [Test]
    public void Parser_WithMinimumValuesAndValidationOn_ReturnsFalse()
    {
        // Minimum values should be treated as invalid when validation is on
        var validData = new byte[] { 0x06, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x4C, 0x88, 0x4F };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }

    [Test]
    public void Parser_WithInvalidSensorValues_ReturnsFalse()
    {
        // Test vector for invalid values (all sensors unavailable)
        var invalidData = new byte[] { 0x06, 0x80, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

        var valid = _parser.TryParseMeasurements(invalidData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }

    [Test]
    public void Parser_WithVOCAndNOXSplitBits_ParsesCorrectly()
    {
        // Test that 9-bit VOC and NOX values are parsed correctly
        // VOC = 257 (0b100000001) = MSB: 128, LSB: 1
        // NOX = 256 (0b100000000) = MSB: 128, LSB: 0
        // Flags byte: bit 6 (VOC LSB) = 1, bit 7 (NOX LSB) = 0 => 0x40
        var validData = new byte[] { 0x06, 0x17, 0x0C, 0x56, 0x68, 0xC7, 0x9E, 0x00, 0x70, 0x00, 0xC9, 0x80, 0x80, 0xD9, 0x00, 0xCD, 0x40, 0x4C, 0x88, 0x4F };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.AirQuality.VolatileOrganicCompoundsIndex.Should().Be(257);
        measurements.AirQuality.NitrogenOxidesIndex.Should().Be(256);
    }

    [Test]
    public void Parser_WithZeroLuminosity_ReturnsNull()
    {
        // Luminosity code 0 means no data
        var validData = new byte[] { 0x06, 0x17, 0x0C, 0x56, 0x68, 0xC7, 0x9E, 0x00, 0x70, 0x00, 0xC9, 0x05, 0x01, 0x00, 0x00, 0xCD, 0x00, 0x4C, 0x88, 0x4F };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Luminosity.Should().BeNull();
    }

    [Test]
    public void Parser_WithLuminosityCode254_ReturnsMaximum()
    {
        // Luminosity code 254 means maximum value
        var validData = new byte[] { 0x06, 0x17, 0x0C, 0x56, 0x68, 0xC7, 0x9E, 0x00, 0x70, 0x00, 0xC9, 0x05, 0x01, 0xFE, 0x00, 0xCD, 0x00, 0x4C, 0x88, 0x4F };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Luminosity.Should().Be(65535m);
    }
}
