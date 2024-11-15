using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest TagGroups from Json files
/// </summary>
public class TagGroupJsonIngestService : BaseJsonIngestService<TagGroup, ITagGroupService>, IIngestService<TagGroup, ITagGroupService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TagGroupJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from TagGroup</param>
    /// <param name="config">JsonIngestConfig</param>
    public TagGroupJsonIngestService(ITagGroupService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters)
    {
        LoadTranslations = true;
    }
}
