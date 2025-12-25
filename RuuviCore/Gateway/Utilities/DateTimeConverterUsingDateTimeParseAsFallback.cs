using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace net.jommy.RuuviCore.Gateway.Utilities;

public class DateTimeConverterUsingDateTimeParseAsFallback : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Debug.Assert(typeToConvert == typeof(DateTime));

        if (!reader.TryGetDateTime(out var value))
        {
            value = DateTime.Parse(reader.GetString());
        }

        return value;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("O"));
    }
}