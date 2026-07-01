using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Extended delegation
/// </summary>
public class ResourceQueue : BaseResourceQueue
{
    /// <summary>
    /// Json containing the instance/app/resource
    /// </summary>
    public string ResourceIdentifier { get; set; }
}
