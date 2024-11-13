using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest Tags from Json files
/// </summary>
public class TagJsonIngestService : BaseJsonIngestService<Tag, ITagService>, IIngestService<Tag, ITagService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Tag</param>
    /// <param name="config">JsonIngestConfig</param>
    public TagJsonIngestService(ITagService service, IOptions<JsonIngestConfig> config) : base(service, config)
    {
        LoadTranslations = true;
    }
}
