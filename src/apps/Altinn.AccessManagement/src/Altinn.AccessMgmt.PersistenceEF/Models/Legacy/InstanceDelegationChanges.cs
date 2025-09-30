using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public class InstanceDelegationChanges : BaseInstanceDelegationChanges
{
    public Resource Resource { get; set; }
}

[NotMapped]
public class BaseInstanceDelegationChanges : CommonDelegationChanges
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
