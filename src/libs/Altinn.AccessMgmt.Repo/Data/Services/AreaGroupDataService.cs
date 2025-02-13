using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
        Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    }
}
