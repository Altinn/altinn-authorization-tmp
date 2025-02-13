using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for PackageResource
/// </summary>
public class PackageResourceDataService : ExtendedRepository<PackageResource, ExtPackageResource>, IPackageResourceService
{
    /// <summary>
    /// Data service for PackageResource
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public PackageResourceDataService(IDbExtendedRepo<PackageResource, ExtPackageResource> repo) : base(repo)
    //{
    //    Join<Package>(t => t.PackageId, t => t.Id, t => t.Package);
    //    Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    //}
    public PackageResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
