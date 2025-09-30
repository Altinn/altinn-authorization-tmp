using System.ComponentModel.DataAnnotations.Schema;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

public class DelegationChanges : BaseDelegationChanges
{
    public Resource AltinnApp { get; set; } // AltinnApp?
}

/// <summary>
/// This model describes a delegation change as stored in the Authorization postgre DelegationChanges table.
/// </summary>
[NotMapped]
public class BaseDelegationChanges : CommonDelegationChanges
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
