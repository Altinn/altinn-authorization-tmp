using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for EntityGroup
/// </summary>
public class AreaGroupDataService : BaseExtendedDataService<AreaGroup, ExtAreaGroup>, IAreaGroupService
{
    /// <summary>
    /// Data service for EntityGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AreaGroupDataService(IDbExtendedRepo<AreaGroup, ExtAreaGroup> repo) : base(repo) 
    {
        ExtendedRepo.Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    }
}
