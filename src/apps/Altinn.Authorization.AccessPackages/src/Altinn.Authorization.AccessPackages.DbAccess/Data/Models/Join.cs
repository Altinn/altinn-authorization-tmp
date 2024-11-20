namespace Altinn.Authorization.AccessPackages.DbAccess.Data.Models;

/// <summary>
/// Join definition
/// </summary>
public class Join
{
    /// <summary>
    /// Alias
    /// </summary>
    public string Alias { get; set; }

    /// <summary>
    /// Join is optional
    /// </summary>
    public bool Optional { get; set; }

    /// <summary>
    /// DbObject that holds the refrenceId
    /// e.g. Entity
    /// </summary>
    public ObjectDefinition BaseObj { get; set; }

    /// <summary>
    /// Refrence column on BaseObj
    /// e.g. GroupId
    /// </summary>
    public string BaseJoinProperty { get; set; }

    /// <summary>
    /// DbObject that is refrenced
    /// e.g. Group
    /// </summary>
    public ObjectDefinition JoinObj { get; set; }

    /// <summary>
    /// Column in JoinObj that is refrenced
    /// e.g. Id
    /// </summary>
    public string JoinProperty { get; set; }

    /// <summary>
    /// Additional filter on for the join statement
    /// e.g. delete is null
    /// </summary>
    public List<GenericFilter> Filter { get; set; } = [];
}
