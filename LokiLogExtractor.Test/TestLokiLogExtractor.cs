using NUnit.Framework;
using System;
using LokiLogExtractor;

[TestFixture]
public class LokiTimeHelperTests
{
    [Test]
    public void ConvertsDateTimeOffsetToNanoseconds()
    {
        var dto = new DateTimeOffset(2024, 01, 01, 00, 00, 00, TimeSpan.Zero);

        long result = LokiTimeHelper.ToUnixNanoseconds(dto);

        // January 1, 2024 Unix time in seconds = 1704067200
        Assert.That(result, Is.EqualTo(1704067200L * 1_000_000_000L));
    }
}