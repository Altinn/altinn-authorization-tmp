namespace Altinn.AccessMgmt.Models;

/// <summary>
/// Policy action connection
/// </summary>
public class PolicyComponent
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
    /// Component identity
    /// </summary>
    public Guid ComponentId { get; set; }
}

/// <summary>
/// Extended policy action
/// </summary>
public class ExtPolicyAction : PolicyComponent
{
    /// <summary>
    /// Resource policy
    /// </summary>
    public Policy Policy { get; set; }

    /// <summary>
    /// Element action
    /// </summary>
    public Component Component { get; set; }
}
