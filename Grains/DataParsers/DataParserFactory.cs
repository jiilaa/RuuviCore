using System;
using System.Collections.Generic;

namespace net.jommy.RuuviCore.Grains.DataParsers
{
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
                [5] = new RAWv2Parser()
            };
        }

        public static IRuuviDataParser GetParser(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return null;
            }

            if (!DataParsers.TryGetValue(data[0], out var parser))
            {
                throw new NotImplementedException($"A parser for protocol version {data[0]} not implemented.");
            }

            return parser;
        }
    }
}
