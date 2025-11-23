using System;
using System.Collections.Generic;

namespace LokiLogExtractor;

public static class LokiLogFlattener
{
    public static IReadOnlyList<LokiLogEntry> Flatten(LokiQueryResponse response)
    {
        var entries = new List<LokiLogEntry>();

        foreach (var stream in response.Data.Result)
        {
            foreach (var value in stream.Values)
            {
                if (value.Length < 2)
                {
                    continue;
                }

                string timestampString = value[0];
                string line = value[1];

                if (!long.TryParse(timestampString, out long nanoseconds))
                {
                    continue;
                }

                DateTimeOffset timestamp = LokiTimeHelper.FromUnixNanoseconds(nanoseconds);

                var entry = new LokiLogEntry
                {
                    Timestamp = timestamp,
                    Line = line,
                    Labels = new Dictionary<string, string>(stream.Stream)
                };

                entries.Add(entry);
            }
        }

        return entries;
    }
}