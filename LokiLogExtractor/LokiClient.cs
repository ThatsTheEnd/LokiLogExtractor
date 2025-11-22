using System;
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

public sealed class LokiClient : ILokiClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _baseAddress;
    private bool _disposed;

    public LokiClient(string baseAddress, HttpClient? httpClient = null)
    {
        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("Base address must not be null or empty.", nameof(baseAddress));
        }

        _baseAddress = baseAddress.TrimEnd('/');
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<HttpResponseMessage> QueryLogsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new ArgumentException("Query must not be null or empty.", nameof(query));
        }

        if (end <= start)
        {
            throw new ArgumentException("End timestamp must be after start timestamp.");
        }

        long startNs = LokiTimeHelper.ToUnixNanoseconds(start);
        long endNs   = LokiTimeHelper.ToUnixNanoseconds(end);

        var uri = $"{_baseAddress}/loki/api/v1/query_range" +
                  $"?query={Uri.EscapeDataString(query)}" +
                  $"&start={startNs}" +
                  $"&end={endNs}" +
                  "&limit=1000";

        return await _httpClient.GetAsync(uri, cancellationToken);
    }

    private static long ToUnixNanoseconds(DateTimeOffset timestamp)
    {
        long unixMilliseconds = timestamp.ToUnixTimeMilliseconds();
        return unixMilliseconds * 1_000_000L;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _httpClient.Dispose();
        _disposed = true;
    }
}