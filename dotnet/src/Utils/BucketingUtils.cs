namespace OmniGraphInterview.Utils;

/// <summary>
/// Class for helper methods to bucket e.g., queries
/// </summary>
public static class BucketingUtils
{
    /// <summary>
    /// Create a collection of buckets from an initial collection of size binSize
    /// E.g.,
    /// <example>
    /// var initial = new[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
    /// var binSize = 3;
    /// var result = BucketCollection(initial, binSize);
    /// var expected = new[] {new[] {1, 2, 3}, new[] {4, 5, 6}, new[] {7, 8, 9}, new[] {10}};
    /// </example>
    /// </summary>
    /// <param name="initial"></param>
    /// <param name="bucketSize"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[][] BucketCollection<T>(T[] initial, int bucketSize)
    {
        var bins = new List<T[]>();
        for (var i = 0; i < initial.Length; i += bucketSize)
        {
            var bin = initial.Skip(i).Take(bucketSize).ToArray();
            bins.Add(bin);
        }

        return bins.ToArray();
    }

}
