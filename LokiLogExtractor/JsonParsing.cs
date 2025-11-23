using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LokiLogExtractor;

public static class LokiResponseParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static LokiQueryResponse ParseQueryResponse(string json)
    {
        LokiQueryResponse? result = JsonSerializer.Deserialize<LokiQueryResponse>(json, Options);

        if (result is null)
        {
            throw new InvalidOperationException("Failed to deserialize Loki query response.");
        }

        return result;
    }
}

public sealed class LokiLogEntry
{
    public DateTimeOffset Timestamp { get; init; }

    public string Line { get; init; } = string.Empty;

    public IReadOnlyDictionary<string, string> Labels { get; init; } = new Dictionary<string, string>();
}