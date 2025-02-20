using Akka;
using Akka.Streams.Dsl;

namespace OmniGraphInterview.Utils;

/// <summary>
/// Utilities for easy stream management
/// </summary>
public static class StreamUtilities
{
    /// <summary>
    /// Deduplicate a stream of records based on a string mapping delegate
    /// This emits running deduplicated values rather than Aggregating them first and then emitting them.
    /// If the string delegate returns null, the record is always emitted.
    /// The overhead of this deduplication is 1 string per unique record that is not null-grouped.
    /// </summary>
    /// <param name="stringDelegate"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Flow<T, T, NotUsed> Deduplicate<T>(Func<T, string?> stringDelegate)
    {
        return Flow.Create<T>()
            .StatefulSelectMany<T, T, T, NotUsed>(() =>
            {
                var seen = new HashSet<string>();
                return rec =>
                {
                    var setId = stringDelegate(rec);
                    if (setId == null || seen.Add(setId)) return [rec];
                    return [];
                };
            });
    }
}
