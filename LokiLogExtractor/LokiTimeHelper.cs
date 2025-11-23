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

    public static DateTimeOffset FromUnixNanoseconds(long nanoseconds)
    {
        long seconds = nanoseconds / 1_000_000_000;
        long nanosRemainder = nanoseconds % 1_000_000_000;
        long ticksRemainder = nanosRemainder / 100; // 1 tick = 100 ns

        var baseTime = DateTimeOffset.FromUnixTimeSeconds(seconds);
        return baseTime.AddTicks(ticksRemainder);
    }
}