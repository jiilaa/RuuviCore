using System;

using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Protocol.DataParsers;

public interface IRuuviDataParser
{
    bool TryParseMeasurements(ReadOnlySpan<byte> data, bool validateValues, out MeasurementDTO measurements);
}
