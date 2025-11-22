using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using LokiLogExtractor;

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
    
    [Test]
    public void QueryLogsAsync_ThrowsWhenEndIsNotAfterStart()
    {
        var client = new LokiLogExtractor.LokiClient("http://localhost:3100");

        var start = DateTimeOffset.UtcNow;
        var endSame = start;
        var endBefore = start.AddMinutes(-1);
        var query = "{job=\"dummy\"}";

        Assert.That(
            async () => await client.QueryLogsAsync(start, endSame, query, CancellationToken.None),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("End timestamp must be after start timestamp"));

        Assert.That(
            async () => await client.QueryLogsAsync(start, endBefore, query, CancellationToken.None),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("End timestamp must be after start timestamp"));
    }

    [Test]
    public async Task QueryLogsAsync_UsesExpectedQueryRangeEndpointAndParameters()
    {
        var handler = new CapturingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = new LokiLogExtractor.LokiClient("http://localhost:3100", httpClient);

        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMinutes(5);
        var query = "{job=\"dummy\"}";

        handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        };

        await client.QueryLogsAsync(start, end, query, CancellationToken.None);

        Assert.That(handler.LastRequest, Is.Not.Null, "No HTTP request was captured.");
        Assert.That(handler.LastRequest!.Method, Is.EqualTo(HttpMethod.Get));

        var uri = handler.LastRequest.RequestUri;
        Assert.That(uri, Is.Not.Null);

        Assert.That(uri!.AbsolutePath, Is.EqualTo("/loki/api/v1/query_range"));

        var queryString = uri.Query.TrimStart('?');

        Assert.That(queryString, Does.Contain("query="));
        Assert.That(queryString, Does.Contain("start="));
        Assert.That(queryString, Does.Contain("end="));
        Assert.That(queryString, Does.Contain("limit="));

        Assert.That(queryString, Does.Contain(Uri.EscapeDataString(query)));
    }

    [Test]
    public async Task QueryLogsAsync_PassesThroughResponseContentFromHttpClient()
    {
        var handler = new CapturingHttpMessageHandler();
        var httpClient = new HttpClient(handler);
        var client = new LokiLogExtractor.LokiClient("http://localhost:3100", httpClient);

        var start = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMinutes(5);
        var query = "{job=\"dummy\"}";

        const string expectedJson = "{\"status\":\"success\",\"data\":{\"resultType\":\"streams\",\"result\":[]}}";

        handler.ResponseToReturn = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedJson)
        };

        var response = await client.QueryLogsAsync(start, end, query, CancellationToken.None);

        Assert.That(response.IsSuccessStatusCode, Is.True);

        var content = await response.Content.ReadAsStringAsync();
        Assert.That(content, Is.EqualTo(expectedJson));
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        public HttpResponseMessage ResponseToReturn { get; set; } =
            new HttpResponseMessage(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(ResponseToReturn);
        }
    }
}