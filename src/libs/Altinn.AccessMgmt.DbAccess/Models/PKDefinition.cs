namespace Altinn.AccessMgmt.DbAccess.Models;

/// <summary>
/// Primary Key definition
/// </summary>
public class PKDefinition
{
    /// <summary>
    /// Properties included in primary key
    /// </summary>
    public Dictionary<string, Type> Properties { get; set; }
}
