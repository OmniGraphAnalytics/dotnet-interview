using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests;

/// <summary>
/// Request for reading recommendations by top N across multiple categories.
/// </summary>
public class ReadMultiCategoriesRequest<T> : ReadByStubBase<T> where T : OmniPulseRecommendation
{
    /// <summary>
    /// Categories to include in the recommendations.
    /// </summary>
    public string[] Categories { get; set; } = [];
}
