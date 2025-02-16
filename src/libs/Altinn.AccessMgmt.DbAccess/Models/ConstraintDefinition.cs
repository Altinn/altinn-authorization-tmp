namespace Altinn.AccessMgmt.DbAccess.Models;

/// <summary>
/// Represents a constraint definition in a database table
/// </summary>
public class ConstraintDefinition
{
    /// <summary>
    /// The name of the constraint
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The type the constraint is base on
    /// </summary>
    public Type Type { get; set; }

    /// <summary>
    /// The columns that the constraint is based on
    /// </summary>
    public List<string> Columns { get; set; }

    /// <summary>
    /// Is the constraint unique
    /// </summary>
    public bool IsUnique { get; set; }
}
