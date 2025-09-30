using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public class ResourceRegistryDelegationChanges : BaseResourceRegistryDelegationChanges
{
    public Resource Resource { get; set; }
}

[NotMapped]
public class BaseResourceRegistryDelegationChanges : CommonDelegationChanges
{
    /// <summary>
    /// Gets or sets the delegation change id
    /// </summary>
    public int ResourceRegistryDelegationChangeId { get; set; }

    /// <summary>
    /// Gets or sets the resource id.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

}
