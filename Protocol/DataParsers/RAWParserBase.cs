using System;
using System.Buffers.Binary;
using System.IO;

using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Protocol.DataParsers;

public abstract class RAWParserBase : IRuuviDataParser
{
    protected abstract byte VersionNumber { get; }
    protected abstract int DataLength { get; }

    public abstract bool TryParseMeasurements(ReadOnlySpan<byte> data, bool validateValues, out MeasurementDTO measurements);

    protected void EnsureVersionAndLength(ReadOnlySpan<byte> data)
    {
        if (data.Length != DataLength)
        {
            throw new InvalidDataException($"Invalid length for version {VersionNumber} data. Expected {DataLength}, actual {data.Length}.");
        }

        if (data[0] != VersionNumber)
        {
            throw new InvalidDataException($"Invalid data version. Expected: {VersionNumber}, actual: {data[0]}.");
        }
    }

    protected static int TwosComplement(ReadOnlySpan<byte> data, int bits)
    {
        int value = BinaryPrimitives.ReadUInt16BigEndian(data);
        if ((value & (1 << (bits - 1))) != 0)
        {
            value -= (1 << bits);
        }

        return value;
    }

    protected static Acceleration ParseAcceleration(ReadOnlySpan<byte> accelerationBytes)
    {
        return new Acceleration
        {
            XAxis = TwosComplement(accelerationBytes.Slice(0), 16) / 1000m,
            YAxis = TwosComplement(accelerationBytes.Slice(2), 16) / 1000m,
            ZAxis = TwosComplement(accelerationBytes.Slice(4), 16) / 1000m
        };
    }
}
