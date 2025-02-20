using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests;

/// <summary>
/// Simple read all matching recommendations request.
/// </summary>
public class ReadByStubRequest<T> : ReadByStubBase<T> where T : OmniPulseRecommendation;

