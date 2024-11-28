using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<Assignment>(t => t.AssignmentId, t => t.Id, t => t.Assignment);
        ExtendedRepo.Join<Group>(t => t.GroupId, t => t.Id, t => t.Group);
    }
}
