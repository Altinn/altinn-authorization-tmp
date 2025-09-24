namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

/// <summary>
/// This model describes a delegation change as stored in the Authorization postgre DelegationChanges table.
/// </summary>
public class DelegationChanges : BaseDelegationChanges
{
    /// <summary>
    /// Gets or sets the delegation change id
    /// </summary>
    public int DelegationChangeId { get; set; }

    /// <summary>
    /// Gets or sets the Altinn App Id
    /// </summary>
    public int AltinnAppId { get; set; }
}
