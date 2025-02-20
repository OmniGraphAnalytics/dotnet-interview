using System.Text.Json;
using Cassandra;
using OmniGraphInterview.Models.OmniPulse.Base;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;

namespace OmniGraphInterview.Models.OmniPulse.Recommendations.Base;

/// <summary>
/// Represents a customer recommendation (e.g., product description, etc.) that can be muted, accepted, rejected, etc.
/// </summary>
public abstract record OmniPulseRecommendation: OmniPulseEntity
{
    /// <summary>
    /// Short summary (140 chars max) for display on cards
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("summary")]
    public string? Summary { get; init; }

    /// <summary>
    /// URL for the picture to display on the card
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("card_picture_url")]
    public string? CardPictureUrl { get; init; }

    /// <summary>
    /// Alt text for the picture to display on the card
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("card_picture_alt_text")]
    public string? CardPictureAltText { get; init; }

    /// <summary>
    /// Title for the card, e.g., 'Product Description Correction'
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("card_title")]
    public string? CardTitle { get; init; }

    /// <summary>
    /// Subtitle for the card, e.g., product name
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("card_subtitle")]
    public string? CardSubtitle { get; init; }

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>
    /// State of the recommendation
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("state", Type = typeof(string))]
    public RecommendationState State { get; init; } = RecommendationState.PENDING;

    /// <summary>
    /// Recommendation muted by the customer until this time
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("muted_until")]
    public DateTimeOffset? MutedUntil { get; init; }

    /// <summary>
    /// Feedback for the recommendation (e.g., Incorrect, Too long, Too short, etc.)
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("feedback")]
    public string Feedback { get; init; } = string.Empty;

    /// <summary>
    /// Dictionary of lookup data for the object being recommended, e.g., {'product_id': '1234'}
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("lookup", Type = typeof(IDictionary<string, string>))]
    [Cassandra.Mapping.Attributes.Frozen]
    [Cassandra.Mapping.Attributes.SecondaryIndex]
    public SortedDictionary<string, string>? Lookup { get; init; }

    /// <summary>
    /// Is the recommendation muted currently
    /// </summary>
    public bool IsMuted => MutedUntil.HasValue && MutedUntil.Value > DateTimeOffset.UtcNow;

    /// <summary>
    /// Is the recommendation pending decision
    /// </summary>
    public bool IsPending => State == RecommendationState.PENDING;

    /// <summary>
    /// Severity of the recommendation
    /// </summary>
    [Cassandra.Mapping.Attributes.Column("severity")]
    public float Severity { get; init; }

    /// <summary>
    /// Add feedback to the recommendation
    /// </summary>
    /// <param name="feedback"></param>
    /// <returns></returns>
    public abstract RegularStatement AddFeedback(string feedback);

    /// <summary>
    /// Set the edits for the recommendation
    /// </summary>
    /// <param name="edits"></param>
    /// <returns></returns>
    public abstract RegularStatement SetEdits(JsonElement edits);

    /// <summary>
    /// Mute the recommendation until a certain time
    /// </summary>
    /// <param name="mutedUntil"></param>
    /// <returns></returns>
    public abstract RegularStatement MuteUntil(DateTimeOffset mutedUntil);

    /// <summary>
    /// Set the state of the recommendation
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public abstract RegularStatement SetState(RecommendationState state);

    /// <summary>
    /// Get the recommendations for the customer
    /// (requires a "stub" recommendation to be called. stub must contain the partition keys for the recommendation)
    /// Example:
    /// <code>
    ///   var recommendation = new ObjectScopedRecommendation
    ///   {
    ///     AccountId = "1234",
    ///     AnalyticsId = "5678"
    ///   };
    ///   var recommendations = recommendation.GetRecommendations(); // returns a statement to query the recommendations
    /// </code>
    /// </summary>
    /// <returns></returns>
    public abstract RegularStatement GetRecommendations(string[]? fields = null);


    /// <summary>
    /// Get the recommendations for the customer across multiple analyticsIds
    /// (requires a "stub" recommendation to be called. stub must contain the partition keys for the recommendation)
    /// Example:
    /// <code>
    ///   var recommendation = new ObjectScopedRecommendation
    ///   {
    ///     AccountId = "1234",
    ///     AnalyticsId = "5678"
    ///   };
    ///   var recommendations = recommendation.GetRecommendations(); // returns a statement to query the recommendations
    /// </code>
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerable<RegularStatement> GetMultiAnalyticsIdsRecommendations(string[] analyticsIds, string[]? fields = null);

    /// <summary>
    /// Get the recommendations for the customer across multiple categories
    /// Example:
    /// <code>
    ///  var recommendation = new ObjectScopedRecommendation
    ///  {
    ///    AccountId = "1234",
    ///    AnalyticsId = "5678"
    ///  };
    ///  var recommendations = recommendation.GetMultiCategoryRecommendations(new[] { "category1", "category2" }); // returns a statement to query the recommendations
    ///
    /// </code>
    /// </summary>
    /// <param name="categories"></param>
    /// <param name="fields"></param>
    /// <returns></returns>
    public abstract IEnumerable<RegularStatement> GetMultiCategoryRecommendations(string[] categories,
        string[]? fields = null);

    /// <summary>
    /// Get a single recommendation for the customer
    /// (requires a "stub" recommendation to be called. stub must contain the primary keys for the recommendation)
    /// Example:
    /// <code>
    ///   var recommendation = new ObjectScopedRecommendation
    ///   {
    ///     AccountId = "1234",
    ///     AnalyticsId = "5678",
    ///     ObjectClass = "product",
    ///     ObjectId = "1234"
    ///   };
    ///   var recommendations = recommendation.GetRecommendation(); // returns a statement to query the recommendation
    /// </code>
    /// </summary>
    /// <returns></returns>
    public abstract RegularStatement GetRecommendation(string[]? fields = null);


    /// <summary>
    /// Lookup the recommendation based on the lookup data (e.g., {'product_id': '1234'})
    /// (requires a "stub" recommendation to be called. stub must contain the partition keys for the recommendation as well as the lookup data)
    /// Example:
    /// <code>
    ///   var recommendation = new ObjectScopedRecommendation
    ///   {
    ///     AccountId = "1234",
    ///     AnalyticsId = "5678",
    ///     Lookup = new() { { "product_id", "1234" } }
    ///   };
    ///   var recommendations = recommendation.GetByLookup(); // returns a statement to query the recommendation
    /// </code>
    /// </summary>
    /// <returns></returns>
    public abstract RegularStatement GetByLookup(string[]? fields = null);

}

/// <inheritdoc />
public abstract record OmniPulseRecommendation<TChangeEvent> : OmniPulseRecommendation
where TChangeEvent : IOmniPulseRecommendationStateChangeEvent
{

    /// <summary>
    /// Create a state change event for the recommendation
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public abstract TChangeEvent CreateStateChangeEvent(RecommendationState state);
}
