using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Extended ResourceElement
/// </summary>
public class ResourceElement : BaseResourceElement
{
    /// <summary>
    /// Resource
    /// </summary>
    public Resource Resource { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public ResourceElementType Type { get; set; }
}
