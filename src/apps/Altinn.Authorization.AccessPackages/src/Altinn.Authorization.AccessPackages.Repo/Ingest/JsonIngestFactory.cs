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
        providerIngestService = new ProviderJsonIngestService(providerService, config);
        areaIngestService = new AreaJsonIngestService(areaService, config);
        areaGroupIngestService = new AreaGroupJsonIngestService(areaGroupService, config);
        entityTypeIngestService = new EntityTypeJsonIngestService(entityTypeService, config);
        entityVariantIngestService = new EntityVariantJsonIngestService(entityVariantService, config);
        entityVariantRoleIngestService = new EntityVariantRoleJsonIngestService(entityVariantRoleService, config);
        packageIngestService = new PackageJsonIngestService(packageService, config);
        roleIngestService = new RoleJsonIngestService(roleService, config);
        roleMapIngestService = new RoleMapJsonIngestService(roleMapService, config);
        rolePackageIngestService = new RolePackageJsonIngestService(rolePackageService, config);
        tagGroupIngestService = new TagGroupJsonIngestService(tagGroupService, config);
        tagIngestService = new TagJsonIngestService(tagService, config);
    }

    /// <summary>
    /// Ingest all services
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns>IngestResults</returns>
    public async Task<List<IngestResult>> IngestAll(CancellationToken cancellationToken)
    {
        Console.WriteLine("Lets eat some data!");

        var result = new List<IngestResult>();

        result.Add(await areaGroupIngestService.IngestData(cancellationToken));
        result.Add(await areaIngestService.IngestData(cancellationToken));
        result.Add(await providerIngestService.IngestData(cancellationToken));
        result.Add(await entityTypeIngestService.IngestData(cancellationToken));
        result.Add(await entityVariantIngestService.IngestData(cancellationToken));
        result.Add(await packageIngestService.IngestData(cancellationToken));
        result.Add(await roleIngestService.IngestData(cancellationToken));
        result.Add(await roleMapIngestService.IngestData(cancellationToken));
        result.Add(await rolePackageIngestService.IngestData(cancellationToken));
        result.Add(await tagGroupIngestService.IngestData(cancellationToken));
        result.Add(await tagIngestService.IngestData(cancellationToken));
        result.Add(await entityVariantRoleIngestService.IngestData(cancellationToken));

        return result;
    }
}