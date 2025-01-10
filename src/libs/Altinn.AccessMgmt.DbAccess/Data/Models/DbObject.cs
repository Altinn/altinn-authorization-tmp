namespace Altinn.AccessMgmt.DbAccess.Data.Models;

/// <summary>
/// Database Object
/// </summary>
public class DbObject
{
    /// <summary>
    /// Type
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// Schema
    /// </summary>
    public string Schema { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Alias
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DbObject"/> class.
    /// </summary>
    /// <param name="type">Type</param>
    /// <param name="name">Name</param>
    /// <param name="schema">Schema</param>
    /// <param name="alias">Alias</param>
    public DbObject(Type type, string name, string schema, string alias = "")
    {
        Type = type;
        Name = name;
        Schema = schema;
        Alias = string.IsNullOrEmpty(alias) ? name : alias;
    }

    /// <summary>
    /// Gets Sql definition
    /// </summary>
    /// <param name="includeAlias">Include alias (default: true)</param>
    /// <param name="useAsOf">Use AsOf (default: false)</param>
    /// <returns></returns>
    public string GetSqlDefinition(bool includeAlias = true, bool useAsOf = false)
    {
        var res = $"[{Schema}].[{Name}]";
        if (useAsOf)
        {
            res += " FOR SYSTEM_TIME AS OF @_AsOf ";
        }

        if (includeAlias)
        {
            res += $" AS [{Alias}]";
        }

        return res;
    }

    /// <summary>
    /// Gets Postgres definition
    /// </summary>
    /// <param name="includeAlias">Include alias (default: true)</param>
    /// <param name="useAsOf">Use AsOf (default: false)</param>
    /// <returns></returns>
    public string GetPostgresDefinition(bool includeAlias = true, bool useAsOf = false)
    {
        var res = $"{Schema}.{Name}";
        if (useAsOf)
        {
            Console.WriteLine("AsOf feature is not available on postgres");
        }

        if (includeAlias)
        {
            res += $" AS {Alias}";
        }

        return res;
    }
}
