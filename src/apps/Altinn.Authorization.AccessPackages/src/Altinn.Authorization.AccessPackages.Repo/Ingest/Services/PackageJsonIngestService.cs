using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest Packages from Json files
/// </summary>
public class PackageJsonIngestService : BaseJsonIngestService<Package, IPackageService>, IIngestService<Package, IPackageService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PackageJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Package</param>
    /// <param name="config">JsonIngestConfig</param>
    public PackageJsonIngestService(IPackageService service, IOptions<JsonIngestConfig> config) : base(service, config)
    {
        LoadTranslations = true;
    }
}
