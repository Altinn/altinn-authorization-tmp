using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for PackageTag
/// </summary>
public class PackageTagDataService : CrossRepository<PackageTag, ExtPackageTag, Package, Tag>, IPackageTagService
{
    /// <summary>
    /// Data service for PackageTag
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public PackageTagDataService(IDbCrossRepo<Package, PackageTag, Tag> repo) : base(repo) { }
    public PackageTagDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
