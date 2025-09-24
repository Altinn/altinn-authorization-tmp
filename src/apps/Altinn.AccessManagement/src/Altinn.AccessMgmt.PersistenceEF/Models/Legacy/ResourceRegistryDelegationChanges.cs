namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public class ResourceRegistryDelegationChanges : BaseDelegationChanges
{
    /// <summary>
    /// Gets or sets the delegation change id
    /// </summary>
    public int ResourceRegistryDelegationChangeId { get; set; }

    /// <summary>
    /// Gets or sets the resource id.
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the resourcetype.
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Altinn App Id
    /// </summary>
    public int resourceid_fk { get; set; }

    /// <summary>
    /// Gets or sets the Altinn App Id
    /// </summary>
    public int performedbypartyid { get; set; }
}
