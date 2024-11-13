using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest Providers from Json files
/// </summary>
public class ProviderJsonIngestService : BaseJsonIngestService<Provider, IProviderService>, IIngestService<Provider, IProviderService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Role</param>
    /// <param name="config">JsonIngestConfig</param>
    public ProviderJsonIngestService(IProviderService service, IOptions<JsonIngestConfig> config) : base(service, config) { }
}
