using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;

namespace OmniGraphInterview.Services.Data.RecommendationRequests;

/// <summary>
/// Request for looking up recommendations by top N using a lookup dictionary.
/// </summary>
public class LookupTopNRequest<T> : ReadTopNByStubRequestBase<T> where T : OmniPulseRecommendation;
