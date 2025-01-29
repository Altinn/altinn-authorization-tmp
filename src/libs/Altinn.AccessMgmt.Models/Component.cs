namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Component for resource element
/// </summary>
public class Component
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Urn
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Element identifier
    /// </summary>
    public Guid ElementId { get; set; }
}

/// <summary>
/// Extended action
/// </summary>
public class ExtComponent: Component
{
    /// <summary>
    /// Element
    /// </summary>
    public Element Element { get; set; }
}
