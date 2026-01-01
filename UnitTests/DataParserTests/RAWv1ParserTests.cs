using System;
using System.IO;
using AwesomeAssertions;
using net.jommy.RuuviCore.Interfaces;
using net.jommy.RuuviCore.Protocol.DataParsers;

using NUnit.Framework;

namespace UnitTests.DataParserTests;

[TestFixture]
// ReSharper disable once InconsistentNaming
public class RAWv1ParserTests
{
    private RAWv1Parser _parser;

    [SetUp]
    public void SetUp()
    {
        _parser = new RAWv1Parser();
    }

    [Test]
    public void Parser_WithInvalidDataLength_ThrowsInvalidDataException()
    {
        var invalidData = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        Action action = () => _parser.TryParseMeasurements(invalidData, true, out _);

        action.Should().Throw<InvalidDataException>("because data length is invalid.");
    }

    [Test]
    public void Parser_WithInvalidDataVersion_ThrowsInvalidDataException()
    {
        var invalidData = new byte[] { 0x04, 0x29, 0x1A, 0x1E, 0xCE, 0x1E, 0xFC, 0x18, 0xF9, 0x42, 0x02, 0xCA, 0x0B, 0x53 };
        Action action = () => _parser.TryParseMeasurements(invalidData, true, out _);

        action.Should().Throw<InvalidDataException>("because data version is invalid.");
    }

    [Test]
    public void Parser_WithValidData_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x03, 0x29, 0x1A, 0x1E, 0xCE, 0x1E, 0xFC, 0x18, 0xF9, 0x42, 0x02, 0xCA, 0x0B, 0x53 };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(-1m, -1.726m, 0.714m),
            Pressure = 1027.66m,
            Humidity = 20.5m,
            Temperature = 26.3m,
            BatteryVoltage = 2899
        };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
        valid.Should().BeTrue();
    }

    [Test]
    public void Parser_WithMaximumValues_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x03, 0xFF, 0x7F, 0x63, 0xFF, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(32.767m, 32.767m, 32.767m),
            Pressure = 1155.35m,
            Humidity = 127.5m,
            Temperature = 127.99m,
            BatteryVoltage = 65535
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }

    [Test]
    public void Parser_WithMaximumValuesAndValidationOn_ReturnsFalse()
    {
        var validData = new byte[] { 0x03, 0xFF, 0x7F, 0x63, 0xFF, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0xFF, 0xFF };

        var valid =_parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }

    [Test]
    public void Parser_WithMinimumValues_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x03, 0x00, 0xFF, 0x63, 0x00, 0x00, 0x80, 0x01, 0x80, 0x01, 0x80, 0x01, 0x00, 0x00 };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(-32.767m, -32.767m, -32.767m),
            Pressure = 500.00m,
            Humidity = 0m,
            Temperature = -127.99m,
            BatteryVoltage = 0
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }

    [Test]
    public void Parser_WithMinimumValuesAndValidationOn_ReturnsFalse()
    {
        var validData = new byte[] { 0x03, 0x00, 0xFF, 0x63, 0x00, 0x00, 0x80, 0x01, 0x80, 0x01, 0x80, 0x01, 0x00, 0x00 };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }
}
