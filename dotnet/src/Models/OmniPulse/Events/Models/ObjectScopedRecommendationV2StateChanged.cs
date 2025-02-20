using Microsoft.Extensions.Logging;
using OmniGraphInterview.Constants;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;
using Snd.Sdk.Metrics.Base;

namespace OmniGraphInterview.Models.OmniPulse.Events.Models;

/// <summary>
/// Event when an object scoped recommendation's state changes
/// </summary>
public record ObjectScopedRecommendationV2StateChanged : IOmniPulseLoggableEvent,
    IOmniPulseRecommendationStateChangeEvent
{
    /// <summary>
    /// Stub of the recommendation that has changed
    /// </summary>
    public required ObjectScopedRecommendationV2 Stub { get; set; }

    /// <inheritdoc />
    public string EventName => EventConstants.ObjectScopedRecommendationV2StateChanged;

    /// <inheritdoc />
    public string EventId => EventName;

    /// <inheritdoc />
    public void SendMetrics(MetricsService metricsService)
    {
        metricsService.Increment(metricName: $"event.{EventName}", tags: new SortedDictionary<string, string>
        {
            { "AnalyticsId", Stub.AccountId ?? "" },
            { "Category", Stub.Category ?? "" },
            { "ObjectClass", Stub.ObjectClass ?? "" },
            { "State", State.ToString() },
            { nameof(AccountId), AccountId },
        });
    }

    /// <inheritdoc />
    public void Log(ILogger logger)
    {
        logger.LogInformation(
            message:
            "Recommendation state changed: {AnalyticsId}, {Category}, {ObjectClass}, {State}, on Account {AccountId}",
            Stub.AccountId, Stub.Category, Stub.ObjectClass, State.ToString(), AccountId);
    }

    /// <inheritdoc />
    public required string AccountId { get; set; }

    /// <inheritdoc />
    public required RecommendationState State { get; set; }
}
