﻿using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest AreaGroups from Json files
/// </summary>
public class AreaGroupJsonIngestService : BaseJsonIngestService<AreaGroup, IAreaGroupService>, IIngestService<AreaGroup, IAreaGroupService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AreaJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from Role</param>
    /// <param name="config">JsonIngestConfig</param>
    public AreaGroupJsonIngestService(IAreaGroupService service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters) : base(service, config, meters)
    {
        LoadTranslations = true;
    }
}