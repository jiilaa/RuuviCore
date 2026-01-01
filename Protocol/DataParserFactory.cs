using System;
using System.Collections.Generic;

using net.jommy.RuuviCore.Protocol.DataParsers;

namespace net.jommy.RuuviCore.Protocol;

/// <summary>
/// Data formats available: https://github.com/ruuvi/ruuvi-sensor-protocols
/// </summary>
public static class DataParserFactory
{
    private static readonly IDictionary<byte, IRuuviDataParser> DataParsers;

    static DataParserFactory()
    {
        DataParsers = new Dictionary<byte, IRuuviDataParser>
        {
            [3] = new RAWv1Parser(),
            [5] = new RAWv2Parser(),
            [6] = new RuuviProtocol6Parser()
        };
    }

    public static IRuuviDataParser GetParser(byte[] data)
    {
        if (data == null || data.Length == 0)
        {
            return null;
        }

        return DataParsers.TryGetValue(data[0], out var parser)
            ? parser
            : throw new NotImplementedException($"A parser for protocol version {data[0]} not implemented.");
    }
}
