using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Logging;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for AreaGroup
/// </summary>
public class AreaGroupDataService : BaseDataService<AreaGroup>, IAreaGroupService
{
    /// <summary>
    /// Data service for AreaGroup
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public AreaGroupDataService(IDbBasicRepo<AreaGroup> repo) : base(repo) { }
}