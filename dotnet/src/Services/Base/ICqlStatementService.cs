using Akka;
using Akka.Streams.Dsl;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;

namespace OmniGraphInterview.Services.Base;

/// <summary>
/// Service for executing CQL statements.
/// </summary>
public interface ICqlStatementService
{
    /// <summary>
    /// Execute a CQL statement asynchronously.
    /// </summary>
    /// <param name="statementDelegate">Delegate to create a CQL statement based on a table reference.</param>
    /// <param name="profile"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> ExecuteAsync<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Execute a CQL statement asynchronously.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Task<bool> ExecuteAsync<T>(RegularStatement statement, string profile = "default");


    /// <summary>
    /// Get entities from a CQL statement.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns></returns>
    public Source<TOut, NotUsed> GetEntities<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Get entities from a CQL statement.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type</typeparam>
    /// <returns></returns>
    public Source<T, NotUsed> GetEntities<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

        /// <summary>
    /// Get entities from a CQL statement.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type</typeparam>
    /// <returns></returns>
    public Source<T, NotUsed> GetEntities<T>(RegularStatement statement, string profile = "default");

    /// <summary>
    /// Get entities from a CQL statement asynchronously and return as an IEnumerable.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns></returns>
    public Task<IEnumerable<TOut>> GetEntitiesAsync<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Get entities from a CQL statement asynchronously and return as an IEnumerable.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type (also output type if no other type-param added)</typeparam>
    /// <returns></returns>
    public Task<IEnumerable<T>> GetEntitiesAsync<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Get entities from a CQL statement asynchronously and return as an IEnumerable.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type (also output type if no other type-param added)</typeparam>
    /// <returns></returns>
    public Task<IEnumerable<T>> GetEntitiesAsync<T>(RegularStatement statement, string profile = "default");

    /// <summary>
    /// Get a single entity from a CQL statement.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type</typeparam>
    /// <typeparam name="TOut">Output type</typeparam>
    /// <returns></returns>
    public Task<TOut?> GetEntity<T, TOut>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Get a single entity from a CQL statement.
    /// </summary>
    /// <param name="statementDelegate"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type (also output type if no other type-param added)</typeparam>
    /// <returns></returns>
    public Task<T?> GetEntity<T>(Func<Table<T>, RegularStatement> statementDelegate, string profile = "default");

    /// <summary>
    /// Get a single entity from a CQL statement.
    /// </summary>
    /// <param name="statement"></param>
    /// <param name="profile"></param>
    /// <typeparam name="T">Table type (also output type if no other type-param added)</typeparam>
    /// <returns></returns>
    public Task<T?> GetEntity<T>(RegularStatement statement, string profile = "default");

    /// <summary>
    /// Get a page of entities from a CQL statement.
    /// </summary>
    /// <param name="selectEntityDelegate"></param>
    /// <param name="pageSize"></param>
    /// <param name="pagingState"></param>
    /// <param name="profile"></param>
    /// <typeparam name="TOut"></typeparam>
    /// <returns></returns>
    public Task<IPage<TOut>> GetEntityPage<TOut>(Func<Table<TOut>, RegularStatement> selectEntityDelegate,
        int? pageSize = null, byte[]? pagingState = null, string profile = "default");
}
