using System.Data.SqlClient;
using Npgsql;

namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// Generic filter
/// </summary>
public class GenericFilter
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Comparer
    /// </summary>
    public DbOperator Comparer { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public GenericFilter(string key, object value, DbOperator? comparer = null)
    {
        Key = key;
        Comparer = comparer ?? DbOperators.EqualTo;
        Value = value;
    }
}

/// <summary>
/// DbOperators
/// </summary>
public static class DbOperators
{
    /// <summary>
    /// EqualTo
    /// </summary>
    public static DbOperator EqualTo => new("=", "{$Key} = {$Value}");

    /// <summary>
    /// NotEqualTo
    /// </summary>
    public static DbOperator NotEqualTo => new("<>", "{$Key} <> {$Value}");

    /// <summary>
    /// In
    /// </summary>
    public static DbOperator In => new("in", "{$Key} IN({$Value})");

    /// <summary>
    /// NotIn
    /// </summary>
    public static DbOperator NotIn => new("not in", "{$Key} NOT IN({$Value})");

    /// <summary>
    /// Contains
    /// </summary>
    public static DbOperator Contains => new("like", "{$Key} LIKE '%{$}%'");

    /// <summary>
    /// NotContains
    /// </summary>
    public static DbOperator NotContains => new("not like", "{$Key} NOT LIKE '%{$Value}%'");

    /// <summary>
    /// StartsWith
    /// </summary>
    public static DbOperator StartsWith => new("like", "LIKE '{$}%'");

    /// <summary>
    /// EndsWith
    /// </summary>
    public static DbOperator EndsWith => new("like", "LIKE '%{$}'");
}

/// <summary>
/// DbOperator
/// </summary>
/// <param name="name">Name (used for ToString override)</param>
/// <param name="code">Code</param>
public readonly struct DbOperator(string name, string code)
{
    /// <summary>
    /// Name (used for ToString override)
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Code 
    /// </summary>
    public string Code { get; } = code;

    /// <summary>
    /// Returns Name
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Name;
    }
}

/// <summary>
/// Generic Parameter
/// </summary>
/// <param name="key">Key</param>
/// <param name="value">Value</param>
public class GenericParameter(string key, object value)
{
    /// <summary>
    /// Key
    /// </summary>
    public string Key { get; set; } = key;

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; } = value;
}