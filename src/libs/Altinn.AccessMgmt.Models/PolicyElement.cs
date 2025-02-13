namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Policy action connection
/// </summary>
public class PolicyElement
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Policy identity
    /// </summary>
    public Guid PolicyId { get; set; }

    /// <summary>
    /// Element identity
    /// </summary>
    public Guid ElementId { get; set; }
}

/// <summary>
/// Extended policy action
/// </summary>
public class ExtPolicyElement : PolicyElement
{
    /// <summary>
    /// Resource policy
    /// </summary>
    public Policy Policy { get; set; }

    /// <summary>
    /// Element action
    /// </summary>
    public Element Element { get; set; }
}
