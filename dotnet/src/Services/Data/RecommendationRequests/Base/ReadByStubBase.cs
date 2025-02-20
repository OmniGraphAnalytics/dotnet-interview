using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests.Base;

/// <summary>
/// Base request for reading recommendations.
/// </summary>
public abstract class ReadReadBase : IRecommendationReadRequest
{
    /// <summary>
    /// Include muted recommendations.
    /// </summary>
    public bool IncludeMuted { get; set; } = false;

    /// <summary>
    /// Include handled recommendations.
    /// </summary>
    public bool IncludeHandled { get; set; } = false;

    /// <summary>
    /// Include recommendations with severity 0.
    /// </summary>
    public bool IncludeSeverity0 { get; set; } = false;

    /// <summary>
    /// Deduplicate recommendations by group. (lookup["group_id"])
    /// </summary>
    public bool DeduplicateGroups { get; set; } = false;

    /// <summary>
    /// Fields to include in the response.
    /// </summary>
    public string[]? Fields { get; set; } = [];
}

/// <summary>
/// Request-base for reading recommendations.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class ReadByStubBase<T> : ReadReadBase
    where T : OmniPulseRecommendation
{
    /// <summary>
    /// Stub recommendation for the request.
    /// </summary>
    public T? Stub { get; set; }


}
