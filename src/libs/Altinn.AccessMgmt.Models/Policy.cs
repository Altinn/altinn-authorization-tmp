namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Resource policy
/// </summary>
public class Policy
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Resource identity
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// Extended resource policy
/// </summary>
public class ExtPolicy : Policy
{
    /// <summary>
    /// Resource
    /// </summary>
    public Resource Resource { get; set; }
}
