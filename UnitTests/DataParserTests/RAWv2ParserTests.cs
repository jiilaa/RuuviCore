using System;
using System.IO;
using FluentAssertions;
using net.jommy.RuuviCore.Grains.DataParsers;
using net.jommy.RuuviCore.Interfaces;
using NUnit.Framework;

namespace UnitTests.DataParserTests;

[TestFixture]
// ReSharper disable once InconsistentNaming
public class RAWv2ParserTests
{
    private RAWv2Parser _parser;

    [SetUp]
    public void SetUp()
    {
        _parser = new RAWv2Parser();
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
        var invalidData = new byte[] { 0x04, 0x12, 0xFC, 0x53, 0x94, 0xC3, 0x7C, 0x00, 0x04, 0xFF, 0xFC, 0x04, 0x0C, 0xAC, 0x36, 0x42, 0x00, 0xCD, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };
        Action action = () => _parser.TryParseMeasurements(invalidData, false, out _);

        action.Should().Throw<InvalidDataException>("because data version is invalid.");
    }

    [Test]
    public void Parser_WithValidData_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x05, 0x12, 0xFC, 0x53, 0x94, 0xC3, 0x7C, 0x00, 0x04, 0xFF, 0xFC, 0x04, 0x0C, 0xAC, 0x36, 0x42, 0x00, 0xCD, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(0.004m, -0.004m, 1.036m),
            Pressure = 1000.44m,
            Humidity = 53.49m,
            Temperature = 24.3m,
            BatteryVoltage = 2977,
            TransmissionPower = 4,
            MovementCounter = 66,
            SequenceNumber = 205
        };

        var valid = _parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
        valid.Should().BeTrue();
    }

    [Test]
    public void Parser_WithMaximumValues_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x05, 0x7F, 0xFF, 0xFF, 0xFE, 0xFF, 0xFE, 0x7F, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0xFF, 0xDE, 0xFE, 0xFF, 0xFE, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(32.767m, 32.767m, 32.767m),
            Pressure = 1155.34m,
            Humidity = 163.8350m,
            Temperature = 163.835m,
            BatteryVoltage = 3646,
            TransmissionPower = 20,
            MovementCounter = 254,
            SequenceNumber = 65534
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }

    [Test]
    public void Parser_WithMaximumValuesAndValidationOn_ReturnsFalse()
    {
        var validData = new byte[] { 0x05, 0x7F, 0xFF, 0xFF, 0xFE, 0xFF, 0xFE, 0x7F, 0xFF, 0x7F, 0xFF, 0x7F, 0xFF, 0xFF, 0xDE, 0xFE, 0xFF, 0xFE, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };

        var valid =_parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }
        
    [Test]
    public void Parser_WithMinimumValues_ReturnsCorrectMeasurements()
    {
        var validData = new byte[] { 0x05, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x01, 0x80, 0x01, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };
        var expectedMeasurements = new MeasurementDTO
        {
            Acceleration = new Acceleration(-32.767m, -32.767m, -32.767m),
            Pressure = 500m,
            Humidity = 0m,
            Temperature = -163.835m,
            BatteryVoltage = 1600,
            TransmissionPower = -40,
            MovementCounter = 0,
            SequenceNumber = 0
        };

        _parser.TryParseMeasurements(validData, false, out var measurements);

        measurements.Should().BeEquivalentTo(expectedMeasurements);
    }
        
    [Test]
    public void Parser_WithMinimumValuesAndValidationOn_ReturnsFalse()
    {
        var validData = new byte[] { 0x05, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x80, 0x01, 0x80, 0x01, 0x80, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0xCB, 0xB8, 0x33, 0x4C, 0x88, 0x4F };

        var valid =_parser.TryParseMeasurements(validData, true, out var measurements);

        measurements.Should().BeNull();
        valid.Should().BeFalse();
    }
}