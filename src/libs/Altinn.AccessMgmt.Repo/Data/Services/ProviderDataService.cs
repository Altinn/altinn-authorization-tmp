using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

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
