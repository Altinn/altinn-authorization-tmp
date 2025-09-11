using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Request permissions
/// </summary>
public class Request : BaseRequest
{
    /// <summary>
    /// Requested by
    /// </summary>
    public Entity RequestedBy { get; set; }

    /// <summary>
    /// Status
    /// </summary>
    public RequestStatus Status { get; set; }

    /// <summary>
    /// Request assignemnt from 
    /// </summary>
    public Entity From { get; set; }

    /// <summary>
    /// Request assignment to
    /// </summary>
    public Entity To { get; set; }

    /// <summary>
    /// Request assignment with role (e.g. rightholder, agent)
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Entity to delegate via
    /// </summary>
    public Entity? Via { get; set; }

    /// <summary>
    /// Role to via entity
    /// </summary>
    public Role? ViaRole { get; set; }
}
