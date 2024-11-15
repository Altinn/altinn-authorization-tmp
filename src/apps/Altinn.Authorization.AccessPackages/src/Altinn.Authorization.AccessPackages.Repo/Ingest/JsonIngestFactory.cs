using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Ingest.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest;

/// <summary>
/// Json Ingest Factory
/// </summary>
public class JsonIngestFactory : IDatabaseIngest
{
    private readonly ProviderJsonIngestService providerIngestService;
    private readonly AreaJsonIngestService areaIngestService;
    private readonly AreaGroupJsonIngestService areaGroupIngestService;
    private readonly EntityTypeJsonIngestService entityTypeIngestService;
    private readonly EntityVariantJsonIngestService entityVariantIngestService;
    private readonly EntityVariantRoleJsonIngestService entityVariantRoleIngestService;
    private readonly PackageJsonIngestService packageIngestService;
    private readonly RoleJsonIngestService roleIngestService;
    private readonly RoleMapJsonIngestService roleMapIngestService;
    private readonly RolePackageJsonIngestService rolePackageIngestService;
    private readonly TagGroupJsonIngestService tagGroupIngestService;
    private readonly TagJsonIngestService tagIngestService;

    /// <summary>
    /// JsonIngestFactory
    /// </summary>
    /// <param name="config">JsonIngestConfig</param>
    /// <param name="providerService">IProviderService</param>
    /// <param name="areaService">IAreaService</param>
    /// <param name="areaGroupService">IAreaGroupService</param>
    /// <param name="entityTypeService">IEntityTypeService</param>
    /// <param name="entityVariantService">IEntityVariantService</param>
    /// <param name="entityVariantRoleService">IEntityVariantRoleService</param>
    /// <param name="packageService">IPackageService</param>
    /// <param name="roleService">IRoleService</param>
    /// <param name="roleMapService">IRoleMapService</param>
    /// <param name="rolePackageService">IRolePackageService</param>
    /// <param name="tagGroupService">ITagGroupService</param>
    /// <param name="tagService">ITagService</param>
    public JsonIngestFactory(
        IOptions<JsonIngestConfig> config,
        JsonIngestMeters meters,
        IProviderService providerService,
        IAreaService areaService,
        IAreaGroupService areaGroupService,
        IEntityTypeService entityTypeService,
        IEntityVariantService entityVariantService,
        IEntityVariantRoleService entityVariantRoleService,
        IPackageService packageService,
        IRoleService roleService,
        IRoleMapService roleMapService,
        IRolePackageService rolePackageService,
        ITagGroupService tagGroupService,
        ITagService tagService
        )
    {
        // jsonIngestConfig = Config.Value;
        providerIngestService = new ProviderJsonIngestService(providerService, config, meters);
        areaIngestService = new AreaJsonIngestService(areaService, config, meters);
        areaGroupIngestService = new AreaGroupJsonIngestService(areaGroupService, config, meters);
        entityTypeIngestService = new EntityTypeJsonIngestService(entityTypeService, config, meters);
        entityVariantIngestService = new EntityVariantJsonIngestService(entityVariantService, config, meters);
        entityVariantRoleIngestService = new EntityVariantRoleJsonIngestService(entityVariantRoleService, config, meters);
        packageIngestService = new PackageJsonIngestService(packageService, config, meters);
        roleIngestService = new RoleJsonIngestService(roleService, config, meters);
        roleMapIngestService = new RoleMapJsonIngestService(roleMapService, config, meters);
        rolePackageIngestService = new RolePackageJsonIngestService(rolePackageService, config, meters);
        tagGroupIngestService = new TagGroupJsonIngestService(tagGroupService, config, meters);
        tagIngestService = new TagJsonIngestService(tagService, config, meters);
    }

    /// <summary>
    /// Ingest all services
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>IngestResults</returns>
    public async Task<List<IngestResult>> IngestAll(CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.DbAccessSource.StartActivity("IngestAll");
        //// Console.WriteLine("Lets eat some data!");

        var result = new List<IngestResult>();

        a?.AddEvent(new System.Diagnostics.ActivityEvent("areaGroupIngestService"));
        result.Add(await areaGroupIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("areaIngestService"));
        result.Add(await areaIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("providerIngestService"));
        result.Add(await providerIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityTypeIngestService"));
        result.Add(await entityTypeIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantIngestService"));
        result.Add(await entityVariantIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("packageIngestService"));
        result.Add(await packageIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleIngestService"));
        result.Add(await roleIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleMapIngestService"));
        result.Add(await roleMapIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("rolePackageIngestService"));
        result.Add(await rolePackageIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("tagGroupIngestService"));
        result.Add(await tagGroupIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("tagIngestService"));
        result.Add(await tagIngestService.IngestData(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantRoleIngestService"));
        result.Add(await entityVariantRoleIngestService.IngestData(cancellationToken));

        return result;
    }
}
