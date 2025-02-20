using System.Reflection;
using System.Text;
using Cassandra;
using Cassandra.Data.Linq;
using OmniGraphInterview.Models.OmniPulse.Base;
using ColumnAttribute = Cassandra.Mapping.Attributes.ColumnAttribute;

namespace OmniGraphInterview.Utils;

/// <summary>
/// Builds dynamic CQL queries
/// </summary>
public static class DynamicCqlBuilder
{
    /// <summary>
    /// Builds a select statement for a given entity type and fields
    /// Operator can be any valid CQL operator (e.g., =, CONTAINS, IN, etc.)
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="fields"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static SimpleStatement BuildSelect<T>(string accountId, List<string> fields)
        where T : OmniPulseEntity
    {
        var tableName = GetTableName<T>();
        var typeFields = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.MemberType == MemberTypes.Property)
            .ToDictionary(m => m.Name.ToLowerInvariant());
        var columns = fields.Select(f => f.ToLowerInvariant())
            .Select(f => typeFields.TryGetValue(key: f, value: out var field) ? field : null)
            .Where(f => f != null).Select(f => f!);
        var cqlFields = fields.Count > 0
            ? columns.Select(f =>
                    f.GetCustomAttributes(attributeType: typeof(ColumnAttribute), inherit: true) as ColumnAttribute[])
                .Where(c => c?.Length != 0).Select(c => c!.First().Name)
            : ["*"];

        return new SimpleStatement(
            query: $"SELECT {string.Join(separator: ',', values: cqlFields)} FROM {tableName} WHERE account_id = ?",
            accountId);
    }

    /// <summary>
    /// Builds a select statement for a given entity type and fields and where clause as a list of tuples (field, operator, value)
    /// Operator can be any valid CQL operator (e.g., =, CONTAINS, IN, etc.)
    /// </summary>
    /// <param name="fields"></param>
    /// <param name="where"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static SimpleStatement BuildSelectWhere<T>(List<string> fields,
        List<(string Key, string Operator, object Value)> where)
    {
        var tableName = GetTableName<T>();
        var typeFields = typeof(T).GetMembers(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.MemberType == MemberTypes.Property)
            .ToDictionary(m => m.Name.ToLowerInvariant());
        var columns = fields.Select(f => f.ToLowerInvariant())
            .Select(f => typeFields.TryGetValue(key: f, value: out var field) ? field : null)
            .Where(f => f != null).Select(f => f!);
        var cqlFields = fields.Count > 0
            ? columns.Select(f =>
                    f.GetCustomAttributes(attributeType: typeof(ColumnAttribute), inherit: true) as ColumnAttribute[])
                .Where(c => c?.Length != 0).Select(c => c!.First().Name)
            : ["*"];


        var (values, whereStatement) = BuildWhereClause(where);

        return new SimpleStatement(
            query: $"SELECT {string.Join(separator: ',', values: cqlFields)} FROM {tableName} WHERE {whereStatement}",
            values: values);
    }

    private static (object[] values, string whereStatement) BuildWhereClause(List<(string Key, string Operator, object Value)> where)
    {
        var values = new List<object>();
        var whereStatement = string.Join(separator: " AND ", values: where.Select(w =>
        {
            // Workaround for IN operator as we're getting Cassandra.InvalidQueryException: Select on indexed columns and with IN clause for the PRIMARY KEY are not supported
            // Case opened with DataStax
            if (w is { Operator: "IN", Value: object[] inValues })
            {
                values.AddRange(inValues);
                return $"{w.Key} {w.Operator} ({string.Join(separator: ",", values: inValues.Select(_ => "?"))})";
            }
            values.Add(w.Value);
            return $"{w.Key} {w.Operator} ?";
        }));
        return (values.ToArray(), whereStatement);
    }


    /// <summary>
    /// Gets the primary keys for a given CQL entity type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string[] GetPrimaryKeys<T>()
    {
        return typeof(T).GetProperties()
            .Where(p =>
                p.GetCustomAttributes(attributeType: typeof(Cassandra.Mapping.Attributes.PartitionKeyAttribute),
                    inherit: true).Length != 0
                ||
                p.GetCustomAttributes(attributeType: typeof(Cassandra.Mapping.Attributes.ClusteringKeyAttribute),
                    inherit: true).Length != 0)
            .Select(p => p.Name)
            .ToArray();
    }

    /// <summary>
    /// Gets the table name for a given CQL entity type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string GetTableName<T>()
    {
        var tableAttribute =
            typeof(T)
                .GetCustomAttributes(attributeType: typeof(Cassandra.Mapping.Attributes.TableAttribute), inherit: true)
                .FirstOrDefault() as Cassandra.Mapping.Attributes.TableAttribute;
        return tableAttribute?.Name ?? typeof(T).Name;
    }
}
