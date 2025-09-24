namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public class InstanceDelegationChanges : BaseDelegationChanges
{
    /// <summary>
    /// Gets or sets the delegation change id
    /// </summary>
    public int DelegationChangeId { get; set; }

    /// <summary>
    /// Gets or sets the resource id.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resource instance id.
    /// </summary>
    public string? InstanceId { get; set; }
}
