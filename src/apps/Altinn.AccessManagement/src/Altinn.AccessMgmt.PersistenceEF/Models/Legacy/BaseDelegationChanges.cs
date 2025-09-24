using System.ComponentModel.DataAnnotations.Schema;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy.Enums;

namespace Altinn.AccessMgmt.PersistenceEF.Models.Legacy;

/// <summary>
/// This model describes a delegation change as stored in the Authorization postgre DelegationChanges table.
/// </summary>
[NotMapped]
public class BaseDelegationChanges
{
    /// <summary>
    /// Gets or sets the delegation change type
    /// </summary>
    public DelegationChangeType DelegationChangeType { get; set; }

    /// <summary>
    /// Gets or sets the offeredbypartyid, refering to the party id of the user or organization offering the delegation.
    /// </summary>
    public int OfferedByPartyId { get; set; }

    /// <summary>
    /// The uuid of the party the right is on behalf of
    /// </summary>
    public Guid? FromUuid { get; set; }

    /// <summary>
    /// The type of party the right is on behalf of (Person, Organization, SystemUser)
    /// </summary>
    public UuidType FromUuidType { get; set; }

    /// <summary>
    /// Gets or sets the coveredbypartyid, refering to the party id of the organization having received the delegation. Otherwise Null if the recipient is a user.
    /// </summary>
    public int? CoveredByPartyId { get; set; }

    /// <summary>
    /// Gets or sets the coveredbyuserid, refering to the user id of the user having received the delegation. Otherwise Null if the recipient is an organization.
    /// </summary>
    public int? CoveredByUserId { get; set; }

    /// <summary>
    /// The uuid of the party holding the right
    /// </summary>
    public Guid? ToUuid { get; set; }

    /// <summary>
    /// The type of party holding the right
    /// </summary>
    public UuidType ToUuidType { get; set; }

    /// <summary>
    /// Gets or sets the user id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
    /// </summary>
    public int? PerformedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the party id of the user that performed the delegation change (either added or removed rules to the policy, or deleted it entirely).
    /// </summary>
    public int? PerformedByPartyId { get; set; }

    /// <summary>
    /// The uuid of the party that performed the delegation
    /// </summary>
    public string? PerformedByUuid { get; set; }

    /// <summary>
    /// The type of the party that performed the delegation
    /// </summary>
    public UuidType PerformedByUuidType { get; set; }

    /// <summary>
    /// Gets or sets blobstoragepolicypath.
    /// </summary>
    public string BlobStoragePolicyPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the blobstorage versionid
    /// </summary>
    public string BlobStorageVersionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created date and timestamp for the delegation change
    /// </summary>
    public DateTime? Created { get; set; }
}
