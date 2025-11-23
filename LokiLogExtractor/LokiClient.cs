using System;
using System.Collections.Generic;
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

    Task<IReadOnlyList<LokiLogEntry>> GetLogsAsync(
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
        long endNs = LokiTimeHelper.ToUnixNanoseconds(end);

        var uri = $"{_baseAddress}/loki/api/v1/query_range" +
                  $"?query={Uri.EscapeDataString(query)}" +
                  $"&start={startNs}" +
                  $"&end={endNs}" +
                  "&limit=1000";

        return await _httpClient.GetAsync(uri, cancellationToken);
    }

    public async Task<IReadOnlyList<LokiLogEntry>> GetLogsAsync(
        DateTimeOffset start,
        DateTimeOffset end,
        string query,
        CancellationToken cancellationToken = default)
    {
        var response = await QueryLogsAsync(start, end, query, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var uri = response.RequestMessage?.RequestUri?.ToString() ?? "<null>";

            throw new HttpRequestException(
                $"Loki query failed with status {(int)response.StatusCode} {response.StatusCode}. " +
                $"URI: {uri}. Body: {body}");
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        LokiQueryResponse model = LokiResponseParser.ParseQueryResponse(json);
        IReadOnlyList<LokiLogEntry> entries = LokiLogFlattener.Flatten(model);

        return entries;
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