using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Services;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Policy
/// </summary>
public class PolicyDataService : BaseExtendedDataService<Policy, ExtPolicy>, IPolicyService
{
    /// <summary>
    /// Data service for Policy
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public PolicyDataService(IDbExtendedRepo<Policy, ExtPolicy> repo) : base(repo)
    {
        Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    }
}
