using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Ingest EntityVariantRoles from Json files
/// </summary>
public class EntityVariantRoleJsonIngestService : BaseJsonIngestService<EntityVariantRole, IEntityVariantRoleService>, IIngestService<EntityVariantRole, IEntityVariantRoleService>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntityVariantRoleJsonIngestService"/> class.
    /// </summary>
    /// <param name="service">Db repo from EntityVariantRole</param>
    /// <param name="config">JsonIngestConfig</param>
    public EntityVariantRoleJsonIngestService(IEntityVariantRoleService service, IOptions<JsonIngestConfig> config) : base(service, config) { }
}
