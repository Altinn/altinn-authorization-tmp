using Altinn.AccessMgmt.PersistenceEF.Models.Base;

namespace Altinn.AccessMgmt.PersistenceEF.Models;

/// <summary>
/// Extended delegation resource
/// </summary>
public class DelegationResource : BaseDelegationResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// Resource
    /// </summary>
    public Resource Resource { get; set; }

    /// <summary>
    /// AssignmentResource
    /// </summary>
    public AssignmentResource? AssignmentResource { get; set; }

    /// <summary>
    /// AssignmentPackage
    /// </summary>
    public AssignmentPackage? AssignmentPackage { get; set; }

    /// <summary>
    /// RolePackage
    /// </summary>
    public RolePackage? RolePackage { get; set; }

    /// <summary>
    /// Package Resource
    /// </summary>
    public PackageResource? PackageResource { get; set; }

    /// <summary>
    /// Roles Resource
    /// </summary>
    public RoleResource? RoleResource { get; set; }
}
