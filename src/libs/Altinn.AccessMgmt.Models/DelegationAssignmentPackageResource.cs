namespace Altinn.AccessMgmt.Models;

/// <summary>
/// DelegationAssignmentPackageResource
/// </summary>
public class DelegationAssignmentPackageResource
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Delegation identity
    /// </summary>
    public Guid DelegationId { get; set; }

    /// <summary>
    /// AssignmentPackage identity
    /// </summary>
    public Guid AssignmentPackageId { get; set; }

    /// <summary>
    /// PackageResource identity
    /// </summary>
    public Guid PackageResourceId { get; set; }
}

/// <summary>
/// Extended DelegationAssignmentPackageResource
/// </summary>
public class ExtDelegationAssignmentPackageResource : DelegationAssignmentPackageResource
{
    /// <summary>
    /// Delegation
    /// </summary>
    public Delegation Delegation { get; set; }

    /// <summary>
    /// AssignmentPackage
    /// </summary>
    public AssignmentPackage AssignmentPackage { get; set; }

    /// <summary>
    /// PackageResource
    /// </summary>
    public PackageResource PackageResource { get; set; }
}
