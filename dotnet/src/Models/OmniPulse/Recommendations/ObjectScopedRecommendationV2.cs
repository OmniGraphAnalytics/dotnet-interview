using System.Text;
using System.Text.Json;
using Cassandra;
using Cassandra.Mapping.Attributes;
using OmniGraphInterview.Exceptions;
using OmniGraphInterview.Models.OmniPulse.Events.Models;
using OmniGraphInterview.Models.OmniPulse.Events.Models.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Base;
using OmniGraphInterview.Models.OmniPulse.Recommendations.Ephemeral;
using OmniGraphInterview.Utils;

namespace OmniGraphInterview.Models.OmniPulse.Recommendations;

/// <summary>
/// Represents a recommendation scoped to an object (by type and ID)
/// </summary>
[Table(Name = "object_scoped_recommendation_v2")]
public record ObjectScopedRecommendationV2 : OmniPulseRecommendation<ObjectScopedRecommendationV2StateChanged>
{
    /// <summary>
    /// Analytics ID
    /// </summary>
    [Column("analytics_id")]
    [PartitionKey(1)]
    public string? AnalyticsId { get; init; }

    /// <summary>
    /// Object class for the recommendation 'product' or 'sku'
    /// </summary>
    [PartitionKey(2)]
    [Column("object_class")]
    public string? ObjectClass { get; init; }

    /// <summary>
    /// Object ID
    /// </summary>
    [ClusteringKey(0)]
    [Column("object_id")]
    public string? ObjectId { get; init; }

    /// <summary>
    /// Category for the recommendation (sub-category, language, etc.)
    /// </summary>
    [ClusteringKey(1)]
    [Column("category")]
    [SecondaryIndex]
    public string? Category { get; init; }

    /// <summary>
    /// Recommendation data (JSON Object, can include from/to language, etc.)
    /// </summary>
    [Column("recommendation_data", Type = typeof(string))]
    public JsonElement? RecommendationData { get; init; }

    /// <summary>
    /// Metadata for the recommendation (e.g., input data, ...)
    /// </summary>
    [Column("metadata", Type = typeof(string))]
    public JsonElement? Metadata { get; init; }

    /// <summary>
    /// Edited recommendation data (copy of recommendation data with user edits)
    /// </summary>
    [Column("edited_recommendation_data", Type = typeof(string))]
    public JsonElement? EditedRecommendationData { get; init; }

    /// <summary>
    /// Algorithm Version used for generating the recommendation
    /// </summary>
    /// <returns></returns>
    [Column("algorithm_version")]
    public string? AlgorithmVersion { get; init; }

    /// <inheritdoc />
    public override string GetFullyQualifyingName()
    {
        return $"opid://{AccountId}/objectScopedRecommendationV2/{AnalyticsId}/{ObjectClass}/{ObjectId}/{Category}";
    }

    /// <inheritdoc />
    public override RegularStatement AddFeedback(string feedback)
    {
        FieldRequiredException.ThrowIfNull(AccountId);
        FieldRequiredException.ThrowIfNull(AnalyticsId);
        FieldRequiredException.ThrowIfNull(ObjectClass);
        FieldRequiredException.ThrowIfNull(ObjectId);
        FieldRequiredException.ThrowIfNull(Category);
        return new SimpleStatement(
            query:
            $"UPDATE {GetTableName()} SET feedback = ?, updated_at = ? WHERE account_id = ? AND analytics_id = ? AND object_class = ? AND object_id = ? AND category = ?",
            feedback, DateTimeOffset.UtcNow, AccountId, AnalyticsId, ObjectClass, ObjectId, Category);
    }

    /// <inheritdoc />
    public override RegularStatement SetEdits(JsonElement edits)
    {
        FieldRequiredException.ThrowIfNull(AccountId);
        FieldRequiredException.ThrowIfNull(AnalyticsId);
        FieldRequiredException.ThrowIfNull(ObjectClass);
        FieldRequiredException.ThrowIfNull(ObjectId);
        FieldRequiredException.ThrowIfNull(Category);
        return new SimpleStatement(
            query:
            $"UPDATE {GetTableName()} SET edited_recommendation_data = ?, updated_at = ? WHERE account_id = ? AND analytics_id = ? AND object_class = ? AND object_id = ? AND category = ?",
            edits.GetRawText(), DateTimeOffset.UtcNow, AccountId, AnalyticsId, ObjectClass, ObjectId, Category);
    }

    /// <inheritdoc />
    public override RegularStatement MuteUntil(DateTimeOffset mutedUntil)
    {
        FieldRequiredException.ThrowIfNull(AccountId);
        FieldRequiredException.ThrowIfNull(AnalyticsId);
        FieldRequiredException.ThrowIfNull(ObjectClass);
        FieldRequiredException.ThrowIfNull(ObjectId);
        FieldRequiredException.ThrowIfNull(Category);
        return new SimpleStatement(
            query:
            $"UPDATE {GetTableName()} SET muted_until = ?, updated_at = ? WHERE account_id = ? AND analytics_id = ? AND object_class = ? AND object_id = ? AND category = ?",
            mutedUntil, DateTimeOffset.UtcNow, AccountId, AnalyticsId, ObjectClass, ObjectId, Category);
    }

    /// <inheritdoc />
    public override RegularStatement SetState(RecommendationState state)
    {
        FieldRequiredException.ThrowIfNull(AccountId);
        FieldRequiredException.ThrowIfNull(AnalyticsId);
        FieldRequiredException.ThrowIfNull(ObjectClass);
        FieldRequiredException.ThrowIfNull(ObjectId);
        FieldRequiredException.ThrowIfNull(Category);
        return new SimpleStatement(
            query:
            $"UPDATE {GetTableName()} SET state = ?, updated_at = ? WHERE account_id = ? AND analytics_id = ? AND object_class = ? AND object_id = ? AND category = ?",
            state.ToString(), DateTimeOffset.UtcNow, AccountId, AnalyticsId, ObjectClass, ObjectId, Category);
    }

    /// <inheritdoc />
    public override RegularStatement GetRecommendations(string[]? fields = null)
    {
        if (AnalyticsId == null || ObjectClass == null)
            return DynamicCqlBuilder.BuildSelectWhere<ObjectScopedRecommendationV2>(fields: fields?.ToList() ?? [],
                where: [AccountIdDelegate]);

        return GetMultiAnalyticsIdsRecommendations(analyticsIds: [AnalyticsId], fields: fields).First();
    }


    /// <inheritdoc />
    public override IEnumerable<RegularStatement> GetMultiAnalyticsIdsRecommendations(string[] analyticsIds, string[]? fields = null)
    {
        var buckets = BucketingUtils.BucketCollection(initial: analyticsIds, bucketSize: 20);
        foreach (var analyticsIdBucket in buckets)
        {
            var where = new List<(string Key, string Operator, object Value)>
            {
                AccountIdDelegate,
                AnalyticsIdsDelegate(analyticsIdBucket),
                ObjectClassDelegate,
            };

            if (!string.IsNullOrEmpty(ObjectId))
            {
                where.Add(ObjectIdDelegate);
            }

            if (!string.IsNullOrEmpty(Category))
            {
                where.Add(CategoryDelegate);
            }

            yield return DynamicCqlBuilder.BuildSelectWhere<ObjectScopedRecommendationV2>(fields: fields?.ToList() ?? [],
                where: where);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<RegularStatement> GetMultiCategoryRecommendations(string[] categories, string[]? fields = null)
    {
        FieldRequiredException.ThrowIfNull(categories);
        var buckets = BucketingUtils.BucketCollection(initial: categories, bucketSize: 20);
        foreach (var categoryBucket in buckets)
        {
            var where = new List<(string Key, string Operator, object Value)>
            {
                AccountIdDelegate,
                CategoriesDelegate(categoryBucket),
                ObjectClassDelegate,
                AnalyticsIdDelegate,
            };

            if (!string.IsNullOrEmpty(ObjectId))
            {
                where.Add(ObjectIdDelegate);
            }

            yield return DynamicCqlBuilder.BuildSelectWhere<ObjectScopedRecommendationV2>(fields: fields?.ToList() ?? [],
                where: where);
        }
    }

    /// <inheritdoc />
    public override RegularStatement GetRecommendation(string[]? fields = null)
    {
        return DynamicCqlBuilder.BuildSelectWhere<ObjectScopedRecommendationV2>(fields?.ToList() ?? [], where:
        [
           AccountIdDelegate,
           AnalyticsIdDelegate,
           ObjectClassDelegate,
           ObjectIdDelegate,
           CategoryDelegate,
        ]);
    }

    /// <inheritdoc />
    public override RegularStatement GetByLookup(string[]? fields = null)
    {
        FieldRequiredException.ThrowIfNull(Lookup);
        var where = new List<(string Key, string Operator, object Value)>
        {
            AccountIdDelegate,
        };

        if (AnalyticsId is not null && ObjectClass is not null)
        {
            // Note, we need to add these in pair to get full partition key, otherwise it uses accountId & secondary index
            where.Add(AnalyticsIdDelegate);
            where.Add(ObjectClassDelegate);
        }

        foreach (var (key, value) in Lookup)
        {
            where.Add(LookupDelegate(key: key, value: value));
        }
        return DynamicCqlBuilder.BuildSelectWhere<ObjectScopedRecommendationV2>(fields: fields?.ToList() ?? [],
            where: where);
    }

    /// <inheritdoc />
    public override ObjectScopedRecommendationV2StateChanged CreateStateChangeEvent(RecommendationState state)
    {
        FieldRequiredException.ThrowIfNull(AccountId);
        FieldRequiredException.ThrowIfNull(AnalyticsId);
        FieldRequiredException.ThrowIfNull(ObjectClass);
        FieldRequiredException.ThrowIfNull(ObjectId);
        FieldRequiredException.ThrowIfNull(Category);
        return new ObjectScopedRecommendationV2StateChanged
        {
            AccountId = AccountId,
            Stub = CreateStub(),
            State = state,
        };
    }

    private ObjectScopedRecommendationV2 CreateStub()
    {
        return new ObjectScopedRecommendationV2
        {
            AccountId = AccountId,
            AnalyticsId = AnalyticsId,
            ObjectClass = ObjectClass,
            ObjectId = ObjectId,
            Category = Category,
        };
    }

    private (string Key, string Operator, object Value) AccountIdDelegate
    {
        get
        {
            FieldRequiredException.ThrowIfNull(AccountId);
            return ("account_id", "=", AccountId);
        }
    }

    private (string Key, string Operator, object Value) AnalyticsIdDelegate
    {
        get
        {
            FieldRequiredException.ThrowIfNull(AnalyticsId);
            return ("analytics_id", "=", AnalyticsId);
        }
    }

    private (string Key, string Operator, object Value) ObjectClassDelegate
    {
        get
        {
            FieldRequiredException.ThrowIfNull(ObjectClass);
            return ("object_class", "=", ObjectClass);
        }
    }

    private (string Key, string Operator, object Value) ObjectIdDelegate
    {
        get
        {
            FieldRequiredException.ThrowIfNull(ObjectId);
            return ("object_id", "=", ObjectId);
        }
    }

    private (string Key, string Operator, object Value) CategoryDelegate
    {
        get
        {
            FieldRequiredException.ThrowIfNull(Category);
            return ("category", "=", Category);
        }
    }

    private static (string Key, string Operator, object Value) AnalyticsIdsDelegate(string[] analyticsIds) => ("analytics_id", "IN", analyticsIds);
    private static (string Key, string Operator, object Value) CategoriesDelegate(string[] categories) => ("category", "IN", categories);
    private static (string Key, string Operator, object Value) LookupDelegate(string key, string value) => ($"lookup['{key}']", "=", value);
}
