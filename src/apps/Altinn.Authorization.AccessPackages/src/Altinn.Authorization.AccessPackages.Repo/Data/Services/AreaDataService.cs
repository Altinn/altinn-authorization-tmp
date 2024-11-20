using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

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
        ExtendedRepo.Join<AreaGroup>(t => t.GroupId, t => t.Id, t => t.Group);
        ExtendedRepo.Join<Package>(t => t.Id, t => t.AreaId, t => t.Packages, isList: true);
    }
}
