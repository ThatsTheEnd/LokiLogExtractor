using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace LokiClient.Tests;

[TestFixture]
public class LokiClientTests
{
    [Test]
    public async Task QueryLogsAsync_ReturnsSuccessfulStatusCode()
    {
        var client = new LokiLogExtractor.LokiClient("http://localhost:3100");

        var start = DateTimeOffset.UtcNow.AddMinutes(-5);
        var end = DateTimeOffset.UtcNow;
        var query = "{job=\"dummy\"}";

        var response = await client.QueryLogsAsync(start, end, query, CancellationToken.None);

        Assert.That(response.IsSuccessStatusCode, Is.True);
    }
}