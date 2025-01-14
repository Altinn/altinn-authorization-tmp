namespace Altinn.AccessMgmt.DbAccess.Data.Models;

/// <summary>
/// Common DataTypes
/// </summary>
public static class DataTypes
{
    /// <summary>
    /// Guid
    /// </summary>
    public static CommonDataType Guid => new("uniqueidentifier", "uuid");

    /// <summary>
    /// Bool
    /// </summary>
    public static CommonDataType Bool => new("bit", "boolean");

    /// <summary>
    /// Int
    /// </summary>
    public static CommonDataType Int => new("int", "int");

    /// <summary>
    /// String
    /// </summary>
    /// <param name="length">Length (default: 250)</param>
    /// <returns></returns>
    public static CommonDataType String(int length = 250) => new($"nvarchar({length})", "text");

    /// <summary>
    /// StringMax
    /// </summary>
    public static CommonDataType StringMax => new("nvarchar(max)", "text");

    /// <summary>
    /// DateTimeOffset
    /// </summary>
    public static CommonDataType DateTimeOffset => new("datetimeoffset(7)", "timestamptz");
}

/// <summary>
/// Common DataType
/// </summary>
public readonly struct CommonDataType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommonDataType"/> struct.
    /// </summary>
    /// <param name="mssql">MSSQL variant</param>
    /// <param name="postgres">Postgres variant</param>
    public CommonDataType(string mssql, string postgres) : this()
    {
        MsSql = mssql;
        Postgres = postgres;
    }

    /// <summary>
    /// MSSQL variant
    /// </summary>
    public string Postgres { get; }

    /// <summary>
    /// Postgres variant
    /// </summary>
    public string MsSql { get; }
}
