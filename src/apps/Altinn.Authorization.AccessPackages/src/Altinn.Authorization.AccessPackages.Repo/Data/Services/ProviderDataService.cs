using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

/// <summary>
/// Data service for Provider
/// </summary>
public class ProviderDataService : BaseDataService<Provider>, IProviderService
{
    /// <summary>
    /// Data service for Provider
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public ProviderDataService(IDbBasicRepo<Provider> repo) : base(repo) { }
}
