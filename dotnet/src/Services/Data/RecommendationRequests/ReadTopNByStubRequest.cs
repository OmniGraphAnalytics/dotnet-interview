using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests;

/// <summary>
/// Request for reading recommendations by top N.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ReadTopNByStubRequest<T> : ReadTopNByStubRequestBase<T> where T : OmniPulseRecommendation;
