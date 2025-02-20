using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests.Base;

/// <summary>
/// Request for reading recommendations by top N.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ReadTopNByStubRequestBase<T> : ReadByStubBase<T>, ITopNRequest<T>
    where T : OmniPulseRecommendation
{
    /// <summary>
    /// Number of recommendations to return.
    /// </summary>
    public int TopN { get; set; } = 10;

    /// <summary>
    /// Comparison function for the recommendations priority, default is by severity.
    /// </summary>
    public virtual Func<T, T, int> Compare { get; set; } = CompareBySeverity;

    /// <summary>
    /// Comparison function for comparing by severity (most severe first).
    /// </summary>
    public static Func<T, T, int> CompareBySeverity =>
        (a, b) => a.Severity.CompareTo(b.Severity);

    /// <summary>
    /// Comparison function for comparing by age (most recent first).
    /// </summary>
    public static Func<T, T, int> CompareByAge =>
        (a, b) => a.UpdatedAt.GetValueOrDefault(DateTimeOffset.MinValue)
            .CompareTo(b.UpdatedAt.GetValueOrDefault(DateTimeOffset.MinValue));
}
