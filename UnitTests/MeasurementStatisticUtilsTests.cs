using System;

using AwesomeAssertions;

using net.jommy.RuuviCore.Grains.Utils;

using NUnit.Framework;

namespace UnitTests;

[TestFixture]
public class MeasurementStatisticUtilsTests
{
    [Test]
    public void Correct_bucket_start_times_calculated_for_hourly_buckets()
    {
        // Arrange
        var bucketSize = TimeSpan.FromHours(1);
        var expectedBucketStartTime = new DateTime(2025, 12, 31, 10, 00, 00, DateTimeKind.Utc);
        var earliestTime = expectedBucketStartTime;
        var middleTime = new DateTime(2025, 12, 31, 10, 30, 00, DateTimeKind.Utc);
        var latestTime = new DateTime(2025, 12, 31, 10, 59, 59, DateTimeKind.Utc);

        // Act & Assert
        earliestTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
        middleTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
        latestTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
    }

    [Test]
    public void Correct_bucket_start_times_calculated_for_5_minute_buckets()
    {
        // Arrange
        var bucketSize = TimeSpan.FromMinutes(5);
        var expectedBucketStartTime = new DateTime(2025, 12, 31, 10, 05, 00, DateTimeKind.Utc);
        var earliestTime = expectedBucketStartTime;
        var middleTime = new DateTime(2025, 12, 31, 10, 07, 33, DateTimeKind.Utc);
        var latestTime = new DateTime(2025, 12, 31, 10, 09, 59, DateTimeKind.Utc);
        var overTime = new DateTime(2025, 12, 31, 10, 10, 05, DateTimeKind.Utc);

        // Act & Assert
        earliestTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
        middleTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
        latestTime.GetAlignedBucketStartTime(bucketSize).Should().Be(expectedBucketStartTime);
        overTime.GetAlignedBucketStartTime(bucketSize).Should().BeAfter(expectedBucketStartTime);
    }
}
