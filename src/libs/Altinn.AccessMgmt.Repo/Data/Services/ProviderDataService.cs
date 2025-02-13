using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Provider
/// </summary>
public class ProviderDataService : BasicRepository<Provider>, IProviderService
{
    /// <summary>
    /// Data service for Provider
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public ProviderDataService(IDbBasicRepo<Provider> repo) : base(repo) { }
    public ProviderDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
