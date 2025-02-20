using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests;

/// <summary>
/// Request for reading recommendations by top N across multiple analytics IDs.
/// </summary>
public class ReadMultiAnalyticsRequest<T> : ReadByStubBase<T> where T : OmniPulseRecommendation
{
    /// <summary>
    /// Analytics IDs to include in the recommendations.
    /// </summary>
    public string[] AnalyticsIds { get; set; } = [];
}
