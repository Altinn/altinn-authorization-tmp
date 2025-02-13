using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for GroupDelegation
/// </summary>
public class GroupDelegationDataService : BaseExtendedDataService<GroupDelegation, ExtGroupDelegation>, IGroupDelegationService
{
    /// <summary>
    /// Data service for GroupDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public GroupDelegationDataService(IDbExtendedRepo<GroupDelegation, ExtGroupDelegation> repo) : base(repo)
    {
        Join<Assignment>(t => t.FromId, t => t.Id, t => t.From);
        Join<EntityGroup>(t => t.ToId, t => t.Id, t => t.To);
        Join<Assignment>(t => t.SourceId, t => t.Id, t => t.Source);
        Join<Assignment>(t => t.ViaId, t => t.Id, t => t.Via);
    }
}
