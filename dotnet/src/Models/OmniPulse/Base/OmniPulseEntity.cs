using Cassandra;
using Cassandra.Data.Linq;
using OmniGraphInterview.Codecs.CassandraCodecs;

namespace OmniGraphInterview.Models.OmniPulse.Base;

/// <summary>
///     Base class for all OmniPulse entities. (All entities must have an account ID)
/// </summary>
public abstract record OmniPulseEntity : OmniPulseJsonStoredObject
{
    /// <summary>
    ///     Account ID associated with the record (always the first element of the partition key)
    /// </summary>
    [Cassandra.Mapping.Attributes.PartitionKey]
    [Cassandra.Mapping.Attributes.Column("account_id")]
    public string? AccountId { get; set; }


    /// <summary>
    ///     Fully qualified name of the entity.
    ///     Has the format: opid://{AccountId}/{EntityName(s)}/{EntityKey1}/{EntityKey2}/...
    /// </summary>
    /// <returns></returns>
    public abstract string GetFullyQualifyingName();


    /// <summary>
    ///     Get all records for an account.
    /// </summary>
    /// <param name="tbl"></param>
    /// <param name="accountId"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static CqlQuery<T> GetByAccount<T>(Table<T> tbl, string accountId) where T : OmniPulseEntity
    {
        return tbl
            .Where(e => e.AccountId == accountId)
            .AllowFiltering();
    }

    /// <summary>
    ///     Get all records for all accounts. (Use with caution to ensure tenant isolation!)
    /// </summary>
    /// <param name="tbl"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static CqlQuery<T> GetAll<T>(Table<T> tbl) where T : OmniPulseEntity
    {
        return tbl;
    }

    /// <summary>
    ///     Get Basic statistics for a table by account.
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public static Func<Table<T>, SimpleStatement> GetStatisticsQuery<T>(string accountId)
    {
        return table => new SimpleStatement(
            query:
            $"SELECT count(*) as entity_count, max(writetime(name)) as last_write FROM {table.Name} WHERE account_id = ?",
            accountId);
    }

    /// <summary>
    ///     Matched the entity with another entity by comparing the fully qualified name.
    ///     Will always return false if the other entity is null.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Match(OmniPulseEntity? other)
    {
        if (ReferenceEquals(objA: null, objB: other)) return false;
        if (ReferenceEquals(objA: this, objB: other)) return true;
        return GetFullyQualifyingName() == other.GetFullyQualifyingName();
    }


    /// <summary>
    /// Get the table name for the entity.
    /// </summary>
    /// <returns></returns>
    public string GetTableName()
    {
        var tableAttribute =
            GetType()
                .GetCustomAttributes(attributeType: typeof(Cassandra.Mapping.Attributes.TableAttribute), inherit: true)
                .FirstOrDefault() as Cassandra.Mapping.Attributes.TableAttribute;
        return tableAttribute?.Name ?? GetType().Name;
    }
}
