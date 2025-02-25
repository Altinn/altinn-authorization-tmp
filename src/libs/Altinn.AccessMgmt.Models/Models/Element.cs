namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Resource element
/// </summary>
public class Element
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
    /// Urn
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Resource identity
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Element type identifier
    /// </summary>
    public Guid TypeId { get; set; }
}

/// <summary>
/// Extended resource element
/// </summary>
public class ExtElement : Element
{
    /// <summary>
    /// Resource
    /// </summary>
    public Resource Resource { get; set; }

    /// <summary>
    /// Element type 
    /// </summary>
    public ElementType Type { get; set; }
}
