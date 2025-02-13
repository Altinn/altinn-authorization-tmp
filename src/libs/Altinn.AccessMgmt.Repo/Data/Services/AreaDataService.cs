using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Area
/// </summary>
public class AreaDataService : BaseExtendedDataService<Area, ExtArea>, IAreaService
{
    /// <summary>
    /// Data service for Area
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AreaDataService(IDbExtendedRepo<Area, ExtArea> repo) : base(repo)
    {
        Join<AreaGroup>(t => t.GroupId, t => t.Id, t => t.Group);
        Join<Package>(t => t.Id, t => t.AreaId, t => t.Packages, isList: true);
    }
}
