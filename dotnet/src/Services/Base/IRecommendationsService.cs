using System.Text.Json;
using Akka;
using Akka.Streams.Dsl;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;
using OmniGraphInterview.Utils;

namespace OmniGraphInterview.Services.Base;

/// <summary>
/// Service for managing recommendations.
/// </summary>
public interface IRecommendationsService
{
    /// <summary>
    /// Get a single recommendation for the given recommendation stub.
    /// (stub should contain all primary keys)
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="fields"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<T?> GetRecommendation<T>(T stub, string[]? fields = null)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Get recommendations for the given recommendation read request.
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Source<T, NotUsed> GetRecommendations<T>(IRecommendationReadRequest request)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Get Top N recommendations for the given recommendation read request.
    /// </summary>
    /// <param name="request"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Source<TopNHeap<T>, NotUsed> GetTopRecommendations<T>(ITopNRequest<T> request)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Update the state of a recommendation.
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="state"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TChangeEvent"></typeparam>
    /// <returns></returns>
    Task<bool> UpdateRecommendationState<T, TChangeEvent>(T stub, RecommendationState state)
        where T : OmniPulseRecommendation<TChangeEvent>
        where TChangeEvent : IOmniPulseRecommendationStateChangeEvent;

    /// <summary>
    /// Update the edits of a recommendation.
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="edits"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> UpdateRecommendationEdits<T>(T stub, JsonElement edits)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Update the feedback of a recommendation.
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="feedback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> UpdateRecommendationFeedback<T>(T stub, string feedback)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Mute a recommendation until the given date.
    /// </summary>
    /// <param name="stub"></param>
    /// <param name="until"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> MuteRecommendation<T>(T stub, DateTimeOffset until)
        where T : OmniPulseRecommendation;

    /// <summary>
    /// Unmute a recommendation.
    /// </summary>
    /// <param name="stub"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    Task<bool> UnmuteRecommendation<T>(T stub)
        where T : OmniPulseRecommendation;
}



