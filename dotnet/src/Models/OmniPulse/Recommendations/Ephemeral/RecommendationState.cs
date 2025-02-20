namespace OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;

/// <summary>
/// State of the product description recommendation
/// </summary>
public enum RecommendationState
{
    /// <summary>
    /// New unhandled recommendation
    /// </summary>
    PENDING,

    /// <summary>
    /// Recommendation has been accepted by the customer
    /// </summary>
    ACCEPTED,

    /// <summary>
    /// Recommendation has been rejected by the customer
    /// </summary>
    REJECTED,

    /// <summary>
    /// Recommendation has been accepted and handled by the recommendation engine
    /// </summary>
    ACCEPTED_AND_HANDLED,

    /// <summary>
    /// Recommendation has been accepted, but handling failed
    /// </summary>
    ACCEPTED_HANDLING_FAILED,

    /// <summary>
    /// Recommendation has been rejected due to invalidation (e.g., source product no longer available or input changed)
    /// </summary>
    INVALIDATED,

    /// <summary>
    /// Unknown state (fallback)
    /// </summary>
    UNKNOWN,

    /// <summary>
    /// Recommendation has been reverted by the customer (pending handling)
    /// </summary>
    REVERTED,

    /// <summary>
    /// Recommendation has been reverted by the customer and handling failed
    /// </summary>
    REVERTED_HANDLING_FAILED,

    /// <summary>
    /// Recommendation has been reverted by the customer and handled
    /// </summary>
    REVERTED_AND_HANDLED,
}
