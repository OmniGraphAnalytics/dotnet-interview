using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;

namespace OmniGraphInterview.Models.OmniPulse.Events.Models.Base;

/// <summary>
/// Event when an object scoped recommendation's state changes
/// </summary>
public interface IOmniPulseRecommendationStateChangeEvent : IOmniPulseAccountScopedEvent
{
    /// <summary>
    /// State of the recommendation after the change
    /// </summary>
    public RecommendationState State { get; set; }
}
