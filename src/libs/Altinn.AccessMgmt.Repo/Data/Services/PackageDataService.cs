using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Package
/// </summary>
public class PackageDataService : ExtendedRepository<Package, ExtPackage>, IPackageService
{
    /// <summary>
    /// Data service for Package
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public PackageDataService(IDbExtendedRepo<Package, ExtPackage> repo) : base(repo)
    //{
    //    Join<Provider>(t => t.ProviderId, t => t.Id, t => t.Provider);
    //    Join<Area>(t => t.AreaId, t => t.Id, t => t.Area);
    //    Join<EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType);
    //}
    public PackageDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
