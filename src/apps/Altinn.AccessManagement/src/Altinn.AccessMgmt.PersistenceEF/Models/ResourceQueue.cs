using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Resource queue entity used for defining the identifiers to syncronice from the resource registry into the access management database
/// </summary>
public class ResourceQueue : BaseResourceQueue
{
    /// <summary>
    /// The identifier for a given resource in the resource registry.
    /// </summary>
    public string ResourceIdentifier { get; set; }
}
