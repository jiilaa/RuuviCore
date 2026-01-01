using System;
using System.Collections.Generic;
using System.Linq;

using net.jommy.RuuviCore.Interfaces;

namespace net.jommy.RuuviCore.Grains.Utils;

public static class MeasurementStatisticUtils
{
    public static DateTime GetAlignedBucketStartTime(this DateTime timestamp, TimeSpan bucketSize)
    {
        var ticks = timestamp.Ticks / bucketSize.Ticks;
        return new DateTime(ticks * bucketSize.Ticks, timestamp.Kind);
    }

    public static MeasurementDTO CalculateAverageMeasurements(
        this IReadOnlyCollection<MeasurementDTO> measurements,
        DateTime timestamp)
    {
        if (measurements == null || measurements.Count == 0)
        {
            return null;
        }

        var averageMeasurements = new MeasurementDTO
        {
            BatteryVoltage = measurements.Last().BatteryVoltage,
            Humidity = measurements.Select(m => m.Humidity).Average(),
            Pressure = measurements.Select(m => m.Pressure).Average(),
            Temperature = measurements.Select(m => m.Temperature).Average(),
            RSSI = (short)measurements.Select(m => (int)m.RSSI).Average(),
            Timestamp = timestamp,
            SequenceNumber = measurements.Last().SequenceNumber,
            MovementCounter = measurements.Last().MovementCounter
        };
        if (measurements.Any(m => m.AirQuality is not null))
        {
            averageMeasurements.AirQuality = new AirQuality
            {
                ParticulateMatter25 = measurements
                    .Where(m => m.AirQuality?.ParticulateMatter25 != null)
                    .Select(m => m.AirQuality.ParticulateMatter25.Value)
                    .Average(),
                CO2Concentration = (int)double.Round(
                    measurements
                        .Where(m => m.AirQuality?.CO2Concentration != null)
                        .Select(m => m.AirQuality.CO2Concentration.Value)
                        .Average(),
                    0),
                VolatileOrganicCompoundsIndex = (int)double.Round(
                    measurements
                        .Where(m => m.AirQuality?.VolatileOrganicCompoundsIndex != null)
                        .Select(m => m.AirQuality.VolatileOrganicCompoundsIndex.Value)
                        .Average(),
                    0),
                NitrogenOxidesIndex = (int)double.Round(
                    measurements
                        .Where(m => m.AirQuality?.NitrogenOxidesIndex != null)
                        .Select(m => m.AirQuality.NitrogenOxidesIndex.Value)
                        .Average(),
                    0)
            };
        }

        return averageMeasurements;
    }
}
