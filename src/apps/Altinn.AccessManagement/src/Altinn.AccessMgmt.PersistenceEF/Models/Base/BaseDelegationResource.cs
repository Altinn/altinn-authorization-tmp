using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Extensions;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Base;

/// <summary>
/// Resources given to a delegation
/// </summary>
[NotMapped]
public class BaseDelegationResource : BaseAudit
{
    private Guid _id;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDelegationResource"/> class.
    /// </summary>
    public BaseDelegationResource()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id
    {
        get => _id;
        set
        {
            if (!value.IsVersion7Uuid())
            {
                throw new ArgumentException("Id must be a version 7 UUID", nameof(value));
            }

            _id = value;
        }
    }

    /// <summary>
    /// Delegation identifier
    /// </summary>
    public Guid DelegationId { get; set; }

    /// <summary>
    /// Resource identifier
    /// </summary>
    public Guid ResourceId { get; set; }

    /// <summary>
    /// AssignmentPackageId
    /// </summary>
    public Guid AssignmentPackageId { get; set; }

    /// <summary>
    /// AssignmentResourceId
    /// </summary>
    public Guid? AssignmentResourceId { get; set; }

    /// <summary>
    /// Role Package ID
    /// </summary>
    public Guid? RolePackageId { get; set; }

    /// <summary>
    /// Package Resource ID
    /// </summary>
    public Guid? PackageResourceId { get; set; }

    /// <summary>
    /// Role Resource ID
    /// </summary>
    public Guid? RoleResourceId { get; set; }
}
