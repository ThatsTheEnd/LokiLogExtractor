using System;
using System.Collections.Generic;

namespace LokiLogExtractor;

public sealed class LokiQueryResponse
{
    public string Status { get; init; } = string.Empty;

    public LokiQueryData Data { get; init; } = new();
}

public sealed class LokiQueryData
{
    public string ResultType { get; init; } = string.Empty;

    public IReadOnlyList<LokiStreamResult> Result { get; init; } = Array.Empty<LokiStreamResult>();

    public LokiQueryStats? Stats { get; init; }
}

public sealed class LokiStreamResult
{
    // Loki labels for this stream
    public Dictionary<string, string> Stream { get; init; } = new();

    // Each entry: [ "<ns timestamp as string>", "<log line>" ]
    public IReadOnlyList<string[]> Values { get; init; } = Array.Empty<string[]>();
}

public sealed class LokiQueryStats
{
    public LokiSummaryStats? Summary { get; init; }
}

public sealed class LokiSummaryStats
{
    public int TotalEntriesReturned { get; init; }
}