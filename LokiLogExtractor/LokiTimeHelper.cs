using System;

namespace LokiLogExtractor;

public static class LokiTimeHelper
{
    public static long ToUnixNanoseconds(DateTime dateTime)
    {
        var dto = new DateTimeOffset(dateTime.ToUniversalTime());
        long unixSeconds = dto.ToUnixTimeSeconds();
        long nanosWithinSecond = dto.ToUnixTimeMilliseconds() % 1000 * 1_000_000;

        return unixSeconds * 1_000_000_000 + nanosWithinSecond;
    }

    public static long ToUnixNanoseconds(DateTimeOffset dto)
    {
        long unixSeconds = dto.ToUnixTimeSeconds();
        long nanosWithinSecond = dto.ToUnixTimeMilliseconds() % 1000 * 1_000_000;

        return unixSeconds * 1_000_000_000 + nanosWithinSecond;
    }
}