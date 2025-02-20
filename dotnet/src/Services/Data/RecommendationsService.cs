using System.Text.Json;
using Akka;
using Akka.Streams.Dsl;
using OmniGraphInterview.Constants;
using OmniGraphInterview.Exceptions;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;
using OmniGraphInterview.Services.Base;
using OmniGraphInterview.Services.Data.RecommendationRequests;
using OmniGraphInterview.Services.Data.RecommendationRequests.Base;
using OmniGraphInterview.Utils;

namespace OmniGraphInterview.Services.Data;

/// <inheritdoc />
public class RecommendationsService(ICqlStatementService cql, IEventPublisher eventPublisher) : IRecommendationsService
{
    /// <inheritdoc />
    public Task<T?> GetRecommendation<T>(T stub, string[]? fields = null)
        where T : OmniPulseRecommendation
    {
        return cql.GetEntity<T>(statement: stub.GetRecommendation(fields), profile: CqlProfiles.ReadFast.Name);
    }

    /// <inheritdoc />
    public Source<T, NotUsed> GetRecommendations<T>(IRecommendationReadRequest request)
        where T : OmniPulseRecommendation
    {
        return Read<T>(request);
    }

    private Source<T, NotUsed> Read<T>(IRecommendationReadRequest request) where T : OmniPulseRecommendation
    {
        return request switch
        {
            ReadByStubRequest<T> stubRequest => GetByStub(request: stubRequest, fields: stubRequest.Fields),
            ReadMultiAnalyticsRequest<T> multiAnalyticsRequest => GetByAnalyticsIds(
                analyticsIds: multiAnalyticsRequest.AnalyticsIds,
                request: multiAnalyticsRequest,
                fields: multiAnalyticsRequest.Fields),
            ReadMultiCategoriesRequest<T> multiCategoriesRequest => GetByCategories(
                categories: multiCategoriesRequest.Categories,
                request: multiCategoriesRequest,
                fields: multiCategoriesRequest.Fields),
            LookupRequest<T> lookupRequest => GetByLookup(
                request: lookupRequest,
                fields: lookupRequest.Fields),
            _ => throw new InvalidConfigurationException("Unsupported recommendation read request type."),
        };
    }

    /// <inheritdoc />
    public Source<TopNHeap<T>, NotUsed> GetTopRecommendations<T>(ITopNRequest<T> request)
        where T : OmniPulseRecommendation
    {
        var source = request switch
        {
            ReadTopNByStubRequest<T> stubRequest => GetByStub(request: stubRequest, fields: GetQueryFields<T>()),
            ReadTopNMultiAnalyticsRequest<T> analyticsIdsRequest => GetByAnalyticsIds(
                analyticsIds: analyticsIdsRequest.AnalyticsIds,
                request: analyticsIdsRequest,
                fields: GetQueryFields<T>()),
            ReadTopNMultiCategoriesRequest<T> categoriesRequest => GetByCategories(
                categories: categoriesRequest.Categories,
                request: categoriesRequest,
                fields: GetQueryFields<T>()),
            LookupTopNRequest<T> lookupRequestTopN => GetByLookup(
                request: lookupRequestTopN,
                fields: GetQueryFields<T>()),
            _ => throw new InvalidConfigurationException("Unsupported top N recommendation read request type."),
        };

        return source
            .Via(TakeTopNSeverity<T>(request.TopN, request.Compare))
            .Via(RefetchFullRecommendation<T>(fields: request.Fields));
    }

    /// <inheritdoc />
    public async Task<bool> UpdateRecommendationState<T, TChangeEvent>(T stub, RecommendationState state)
        where T : OmniPulseRecommendation<TChangeEvent>
        where TChangeEvent : IOmniPulseRecommendationStateChangeEvent
    {
        await eventPublisher.PublishEvent(stub.CreateStateChangeEvent(state));
        return await cql.ExecuteAsync<T>(statement: stub.SetState(state), profile: CqlProfiles.Default.Name);
    }

    /// <inheritdoc />
    public Task<bool> UpdateRecommendationEdits<T>(T stub, JsonElement edits) where T : OmniPulseRecommendation
    {
        return cql.ExecuteAsync<T>(statement: stub.SetEdits(edits), profile: CqlProfiles.WriteSafe.Name);
    }

    /// <inheritdoc />
    public Task<bool> UpdateRecommendationFeedback<T>(T stub, string feedback)
        where T : OmniPulseRecommendation
    {
        return cql.ExecuteAsync<T>(statement: stub.AddFeedback(feedback), profile: CqlProfiles.Default.Name);
    }

    /// <inheritdoc />
    public Task<bool> MuteRecommendation<T>(T stub, DateTimeOffset until)
        where T : OmniPulseRecommendation
    {
        return cql.ExecuteAsync<T>(statement: stub.MuteUntil(until), profile: CqlProfiles.Default.Name);
    }

    /// <inheritdoc />
    public Task<bool> UnmuteRecommendation<T>(T stub)
        where T : OmniPulseRecommendation
    {
        return cql.ExecuteAsync<T>(statement: stub.MuteUntil(DateTimeOffset.Now), profile: CqlProfiles.Default.Name);
    }

    private Source<T, NotUsed> GetByLookup<T>(ReadByStubBase<T> request, string[]? fields = null)
        where T : OmniPulseRecommendation
    {
        if (request.Stub == null)
        {
            return Source.Empty<T>();
        }

        return cql.GetEntities<T>(statement: request.Stub
                .GetByLookup(ExtendFields<T>(fields)), profile: CqlProfiles.ReadFast.Name)
            .Where(r => request.IncludeMuted || !r.IsMuted)
            .Where(r => request.IncludeHandled || r.IsPending)
            .Where(r => request.IncludeSeverity0 || r.Severity > 0)
            .Via(OptionalDeduplication<T>(request.DeduplicateGroups));
    }

    private Source<T, NotUsed> GetByAnalyticsIds<T>(string[] analyticsIds, string[]? fields, ReadByStubBase<T> request)
        where T : OmniPulseRecommendation
    {
        if (request.Stub == null)
        {
            return Source.Empty<T>();
        }

        return Source.From(request.Stub
                .GetMultiAnalyticsIdsRecommendations(analyticsIds: analyticsIds, fields: ExtendFields<T>(fields)))
            .ConcatMany(q => cql.GetEntities<T>(statement: q, profile: CqlProfiles.ReadFast.Name))
            .Where(r => request.IncludeMuted || !r.IsMuted)
            .Where(r => request.IncludeHandled || r.IsPending)
            .Where(r => request.IncludeSeverity0 || r.Severity > 0)
            .Via(OptionalDeduplication<T>(request.DeduplicateGroups));
    }

    private Source<T, NotUsed> GetByCategories<T>(ReadByStubBase<T> request, string[] categories,
        string[]? fields = null)
        where T : OmniPulseRecommendation
    {
        if (request.Stub == null)
        {
            return Source.Empty<T>();
        }

        return Source.From(request.Stub
                .GetMultiCategoryRecommendations(categories: categories, fields: ExtendFields<T>(fields)))
            .ConcatMany(q => cql.GetEntities<T>(statement: q, profile: CqlProfiles.ReadFast.Name))
            .Where(r => request.IncludeMuted || !r.IsMuted)
            .Where(r => request.IncludeHandled || r.IsPending)
            .Where(r => request.IncludeSeverity0 || r.Severity > 0)
            .Via(OptionalDeduplication<T>(request.DeduplicateGroups));
    }

    private Source<T, NotUsed> GetByStub<T>(ReadByStubBase<T> request, string[]? fields = null)
        where T : OmniPulseRecommendation
    {
        if (request.Stub == null)
        {
            return Source.Empty<T>();
        }

        return cql.GetEntities<T>(statement: request.Stub.GetRecommendations(ExtendFields<T>(fields)),
                profile: CqlProfiles.ReadFast.Name)
            .Where(r => request.IncludeMuted || !r.IsMuted)
            .Where(r => request.IncludeHandled || r.IsPending)
            .Where(r => request.IncludeSeverity0 || r.Severity > 0)
            .Via(OptionalDeduplication<T>(request.DeduplicateGroups));
    }

    private static Flow<T, TopNHeap<T>, NotUsed> TakeTopNSeverity<T>(int size, Func<T, T, int> compare)
        where T : OmniPulseRecommendation
    {
        return Flow.Create<T>()
            .Aggregate(
                zero: new TopNHeap<T>(size: size, compare: compare),
                fold: (heap, r) =>
                {
                    heap.Add(r);
                    return heap;
                });
    }

    private static Flow<T, T, NotUsed> OptionalDeduplication<T>(bool deduplicateGroups)
        where T : OmniPulseRecommendation
    {
        return deduplicateGroups
            ? StreamUtilities.Deduplicate((T r) => r.Lookup?.GetValueOrDefault(key: "group_id"))
            : Flow.Create<T>();
    }

    private static string[]? ExtendFields<T>(string[]? fields = null) where T : OmniPulseRecommendation
    {
        if (fields == null || fields.Length == 0)
        {
            return fields;
        }

        return DynamicCqlBuilder.GetPrimaryKeys<T>()
            .Concat([
                nameof(OmniPulseRecommendation.Severity), nameof(OmniPulseRecommendation.State),
                nameof(OmniPulseRecommendation.UpdatedAt), nameof(OmniPulseRecommendation.MutedUntil),
                nameof(OmniPulseRecommendation.Lookup),
            ]).Concat(fields).Distinct().ToArray();
    }

    private static string[] GetQueryFields<T>() where T : OmniPulseRecommendation
    {
        return DynamicCqlBuilder.GetPrimaryKeys<T>()
            .Concat([
                nameof(OmniPulseRecommendation.Severity), nameof(OmniPulseRecommendation.State),
                nameof(OmniPulseRecommendation.UpdatedAt), nameof(OmniPulseRecommendation.MutedUntil),
                nameof(OmniPulseRecommendation.Lookup),
            ]).ToArray();
    }

    private Flow<TopNHeap<T>, TopNHeap<T>, NotUsed> RefetchFullRecommendation<T>(string[]? fields = null)
        where T : OmniPulseRecommendation
    {
        return Flow.Create<TopNHeap<T>>()
            .SelectAsync(parallelism: Environment.ProcessorCount, asyncMapper: topNHeap => topNHeap
                .SelectAsync(r => cql.GetEntity<T>(statement: r.GetRecommendation(ExtendFields<T>(fields)),
                        profile: CqlProfiles.ReadFast.Name)
                    .ThrowIfNotFound()));
    }
}
