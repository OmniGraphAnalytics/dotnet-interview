using Akka;
using Akka.Streams.Dsl;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using Microsoft.Extensions.Logging;
using OmniGraphInterview.Services.Base;
using Snd.Sdk.Tasks;

namespace OmniGraphInterview.Services.Cql;

/// <inheritdoc />
public class CqlStatementService(ISession session, ILogger<CqlStatementService> logger) : ICqlStatementService
{
    /// <inheritdoc />
    public Task<bool> ExecuteAsync<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default")
    {
        var tableRef = new Table<T>(session: session, config: MappingConfiguration.Global);
        return ExecuteAsync<T>(statement: statementDelegate(tableRef), profile: profile);
    }

    /// <inheritdoc />
    public Task<bool> ExecuteAsync<T>(RegularStatement statement, string profile = "default")
    {
        return session.ExecuteAsync(statement: statement, executionProfileName: profile)
            .TryMap(selector: _ => true, errorHandler: error =>
            {
                LogError(exception: error, query: new Cassandra.Mapping.Cql(cql: statement.QueryString, args: statement.QueryValues));
                return false;
            });
    }

    /// <inheritdoc />
    public Source<TOut, NotUsed> GetEntities<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate,
        string profile = "default")
    {
        var tableRef = new Table<T>(session: session, config: MappingConfiguration.Global);
        return Source.FromTask(GetEntitiesAsync<TOut>(statement: statementDelegate(tableRef), profile: profile))
            .SelectMany(t => t);
    }

    /// <inheritdoc />
    public Source<T, NotUsed> GetEntities<T>(Func<Table<T>, RegularStatement> statementDelegate,
        string profile = "default")
    {
        return GetEntities<T, T>(statementDelegate: statementDelegate, profile: profile);
    }

    /// <inheritdoc />
    public Source<T, NotUsed> GetEntities<T>(RegularStatement statement, string profile = "default")
    {
        return Source.FromTask(GetEntitiesAsync<T>(statement: statement, profile: profile)).SelectMany(t => t);
    }

    /// <inheritdoc />
    public Task<IEnumerable<TOut>> GetEntitiesAsync<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate,
        string profile = "default")
    {
        var tableRef = new Table<T>(session: session, config: MappingConfiguration.Global);
        return GetEntitiesAsync<TOut>(statement: statementDelegate(tableRef), profile: profile);
    }

    /// <inheritdoc />
    public Task<IEnumerable<T>> GetEntitiesAsync<T>(Func<Table<T>, RegularStatement> statementDelegate,
        string profile = "default")
    {
        return GetEntitiesAsync<T, T>(statementDelegate: statementDelegate, profile: profile);
    }

    /// <inheritdoc />
    public Task<IEnumerable<T>> GetEntitiesAsync<T>(RegularStatement statement, string profile = "default")
    {
        var mapper = new Mapper(session: session, config: MappingConfiguration.Global);
        var query = new Cassandra.Mapping.Cql(cql: statement.QueryString, args: statement.QueryValues)
            .WithExecutionProfile(profile);
        return mapper.FetchAsync<T>(query)
            .TryMap(selector: s => s, errorHandler: error =>
            {
                LogError(exception: error, query: query);
                throw error;
            });
    }

    /// <inheritdoc />
    public Task<TOut?> GetEntity<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate,
        string profile = "default")
    {
        var tableRef = new Table<T>(session: session, config: MappingConfiguration.Global);
        return GetEntity<TOut>(statement: statementDelegate(tableRef), profile: profile);
    }

    /// <inheritdoc />
    public Task<T?> GetEntity<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default")
    {
        return GetEntity<T, T>(statementDelegate: statementDelegate, profile: profile);
    }

    /// <inheritdoc />
    public Task<T?> GetEntity<T>(RegularStatement statement, string profile = "default")
    {
        var mapper = new Mapper(session: session, config: MappingConfiguration.Global);
        var query = new Cassandra.Mapping.Cql(cql: statement.QueryString, args: statement.QueryValues)
            .WithExecutionProfile(profile);
        return mapper.FetchAsync<T>(query).Map(t => t.FirstOrDefault())
            .TryMap(selector: s => s, errorHandler: error =>
            {
                LogError(exception: error, query: query);
                throw error;
            });
    }

    /// <inheritdoc />
    public Task<IPage<TOut>> GetEntityPage<TOut>(
        Func<Table<TOut>, RegularStatement> selectEntityDelegate,
        int? pageSize = null,
        byte[]? pagingState = null,
        string profile = "default")
    {
        var tableRef = new Table<TOut>(session: session, config: MappingConfiguration.Global);
        var mapper = new Mapper(session: session, config: MappingConfiguration.Global);
        var selectQuery = selectEntityDelegate(tableRef);
        var query = new Cassandra.Mapping.Cql(cql: selectQuery.QueryString, args: selectQuery.QueryValues)
            .WithOptions(options => options
                .SetPageSize(pageSize.GetValueOrDefault(1000))
                .SetPagingState(pagingState)
            )
            .WithExecutionProfile(profile);
        return mapper.FetchPageAsync<TOut>(query)
            .TryMap(selector: s => s, errorHandler: error =>
            {
                LogError(exception: error, query: query);
                throw error;
            });
    }

    private void LogError(Exception exception, Cassandra.Mapping.Cql query)
    {
        if (exception is InvalidQueryException invalidQueryException)
        {
            logger.LogError(exception: invalidQueryException,
                message: "Invalid CQL query: {query} with arguments: {arguments}", query.Statement, query.Arguments);
        }
    }
}
