using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Contracts;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Represents a request to assign a role from one party to another using an assignment.
/// </summary>
public class BaseRequestAssignment : BaseAudit, IEntityId
{
    /// <summary>
    /// Identifier for the request
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identify the party that is the source of the assignment
    /// </summary>
    public Guid FromId { get; set; }

    /// <summary>
    /// Identify the party that is the target of the assignment
    /// </summary>
    public Guid ToId { get; set; }

    /// <summary>
    /// Identify the role that is being assigned
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Identify the user that requested the assignment
    /// </summary>
    public Guid RequestedById { get; set; }

    /// <summary>
    /// The status of the request (e.g., pending, approved, rejected)
    /// </summary>
    public RequestStatus Status { get; set; }
}
