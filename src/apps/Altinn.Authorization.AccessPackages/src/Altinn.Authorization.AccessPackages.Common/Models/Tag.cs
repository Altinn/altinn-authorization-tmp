namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Tag
/// </summary>
public class Tag
{
    /// <summary>
    /// Id 
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// GroupId
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// ParentId
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Extended tag
/// </summary>
public class ExtTag : Tag
{
    /// <summary>
    /// Group (optional)
    /// </summary>
    public TagGroup? Group { get; set; }

    /// <summary>
    /// Parent (optional)
    /// </summary>
    public Tag? Parent { get; set; }
}