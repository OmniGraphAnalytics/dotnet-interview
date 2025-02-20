using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests.Base;

/// <summary>
/// Request-interface for reading recommendations by top N.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITopNRequest<T> : IRecommendationReadRequest where T : OmniPulseRecommendation
{
    /// <summary>
    /// Number of recommendations to return.
    /// </summary>
    public int TopN { get; set; }

    /// <summary>
    /// Comparison function for the recommendations priority.
    /// </summary>
    public Func<T, T, int> Compare { get; set; }

    /// <summary>
    /// Fields to include in the response.
    /// </summary>
    public string[]? Fields { get; set; }
}
