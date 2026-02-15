using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Represents a request to create an assignment
/// </summary>
public class RequestAssignment : BaseRequestAssignment
{

    /// <summary>
    /// The party that is the source of the assignment
    /// </summary>
    public Entity From { get; set; }

    /// <summary>
    /// The party that is the target of the assignment
    /// </summary>
    public Entity To { get; set; }

    /// <summary>
    /// The role that is being requested assigned
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// The user that requested the assignment
    /// </summary>
    public Entity RequestedBy { get; set; }
}
