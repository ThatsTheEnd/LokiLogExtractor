using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LokiLogExtractor;

public interface ILokiClient
{
    Task<HttpResponseMessage> QueryLogsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        string query,
        CancellationToken cancellationToken = default);
}

public sealed class LokiClient : ILokiClient
{
    private readonly string _baseUrl;

    public LokiClient(string baseUrl)
    {
        _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
    }

    public Task<HttpResponseMessage> QueryLogsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        string query,
        CancellationToken cancellationToken = default)
    {
        var response = new HttpResponseMessage(HttpStatusCode.NotFound);
        return Task.FromResult(response);
    }
}