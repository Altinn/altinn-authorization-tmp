using System.Data;
using System.Text.Json;
using System.Threading;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Repo.Ingest.RagnhildModel;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest;

/// <summary>
/// Json Ingest Factory
/// </summary>
public class JsonIngestFactory
{
    /// <summary>
    /// Configuration
    /// </summary>
    public JsonIngestConfig Config { get; set; }

    private readonly IProviderService providerService;
    private readonly IAreaService areaService;
    private readonly IAreaGroupService areaGroupService;
    private readonly IEntityTypeService entityTypeService;
    private readonly IEntityVariantService entityVariantService;
    private readonly IEntityVariantRoleService entityVariantRoleService;
    private readonly IPackageService packageService;
    private readonly IRoleService roleService;
    private readonly IRoleMapService roleMapService;
    private readonly IRolePackageService rolePackageService;
    private readonly ITagGroupService tagGroupService;
    private readonly ITagService tagService;

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
        Config = config.Value;
        this.providerService = providerService;
        this.areaService = areaService;
        this.areaGroupService = areaGroupService;
        this.entityTypeService = entityTypeService;
        this.entityVariantService = entityVariantService;
        this.entityVariantRoleService = entityVariantRoleService;
        this.packageService = packageService;
        this.roleService = roleService;
        this.roleMapService = roleMapService;
        this.rolePackageService = rolePackageService;
        this.tagGroupService = tagGroupService;
        this.tagService = tagService;
    }

    /// <summary>
    /// Ingest all
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<List<IngestResult>> IngestAll(CancellationToken cancellationToken = default)
    {
        using var a = DbAccess.DbAccessTelemetry.DbAccessSource.StartActivity("IngestAll");

        var result = new List<IngestResult>();

        a?.AddEvent(new System.Diagnostics.ActivityEvent("RolePackagesIngestService"));
        //// result.AddRange(await IngestRolePackages(string.Empty, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("areasAndPackagesIngestService"));
        result.AddRange(await IngestAreasAndPackages(cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("providerIngestService"));
        result.Add(await IngestData<Provider, IProviderService>(providerService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityTypeIngestService"));
        result.Add(await IngestData<EntityType, IEntityTypeService>(entityTypeService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantIngestService"));
        result.Add(await IngestData<EntityVariant, IEntityVariantService>(entityVariantService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleIngestService"));
        result.Add(await IngestData<Role, IRoleService>(roleService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleMapIngestService"));
        result.Add(await IngestData<RoleMap, IRoleMapService>(roleMapService, cancellationToken));

        //a?.AddEvent(new System.Diagnostics.ActivityEvent("rolePackageIngestService"));
        //result.Add(await IngestData<RolePackage, IRolePackageService>(rolePackageService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("tagGroupIngestService"));
        result.Add(await IngestData<TagGroup, ITagGroupService>(tagGroupService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("tagIngestService"));
        result.Add(await IngestData<Tag, ITagService>(tagService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantRoleIngestService"));
        result.Add(await IngestData<EntityVariantRole, IEntityVariantRoleService>(entityVariantRoleService, cancellationToken));

        return result;
    }

    /// <summary>
    /// Ingest single type
    /// </summary>
    /// <typeparam name="T">Type</typeparam>
    /// <typeparam name="TService">Type Data Service</typeparam>
    /// <param name="service">Service</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<IngestResult> IngestData<T, TService>(TService service, CancellationToken cancellationToken)
        where TService : IDbBasicDataService<T>
    {
        var type = typeof(T);
        var translatedItems = new Dictionary<string, List<T>>();

        foreach (var lang in Config.Languages.Distinct())
        {
            var translatedJsonData = await ReadJsonData<T>(lang, cancellationToken: cancellationToken);
            if (translatedJsonData == "[]")
            {
                continue;
            }

            var translatedJsonItems = JsonSerializer.Deserialize<List<T>>(translatedJsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (translatedJsonItems == null)
            {
                // send ut en feil her
                continue;
            }

            translatedItems.Add(lang, translatedJsonItems);
        }

        var jsonData = await ReadJsonData<T>(cancellationToken: cancellationToken);
        if (jsonData == "[]")
        {
            return new IngestResult(type) { Success = false };
        }

        var jsonItems = JsonSerializer.Deserialize<List<T>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        if (jsonItems == null)
        {
            return new IngestResult(type) { Success = false };
        }

        return await IngestData(service, jsonItems, translatedItems, cancellationToken);
    }

    public async Task<IngestResult> IngestData<T, TService>(TService service, List<T> jsonItems, Dictionary<string, List<T>> languageJsonItems, CancellationToken cancellationToken)
        where TService : IDbBasicDataService<T>
    {
        var dbItems = await service.Get();

        Console.WriteLine($"Ingest {typeof(T).Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");

        if (dbItems == null || !dbItems.Any())
        {
            foreach (var item in jsonItems)
            {
                await service.Create(item);
            }
        }
        else
        {
            foreach (var item in jsonItems)
            {
                if (dbItems.Count(t => IdComparer(item, t)) == 0)
                {
                    await service.Create(item);
                    continue;
                }

                if (dbItems.Count(t => PropertyComparer(item, t)) == 0)
                {
                    var id = GetId(item);
                    if (id.HasValue)
                    {
                        await service.Update(id.Value, item);
                    }
                }
            }
        }

        await IngestTranslation(service, languageJsonItems, cancellationToken);

        return new IngestResult(typeof(T)) { Success = true };
    }

    private async Task<List<IngestResult>> IngestRolePackages(string language = "", CancellationToken cancellationToken = default)
    {
        var result = new List<IngestResult>();
        var jsonData = await ReadJsonData("NewRolePackages", language);
        var metaRolePackages = JsonSerializer.Deserialize<List<MetaRolePackage>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        var allpackages = await packageService.Get();
        var allroles = await roleService.Get();
        var uniquePackages = new Dictionary<string, Guid>();
        var uniqueRoles = new Dictionary<string, Guid>();
        var rolePackages = new List<RolePackage>();

        foreach (var rolePackage in metaRolePackages)
        {
            uniquePackages.TryAdd(rolePackage.Tilgangspakke, allpackages.First(x => x.Name == rolePackage.Tilgangspakke).Id);
            foreach (var role in rolePackage.Enhetsregisterroller)
            {
                uniqueRoles.TryAdd(role, allroles.First(x => x.Name.ToLower() == role.ToLower()).Id);
            }
        }

        foreach (var rolePackage in metaRolePackages)
        {
            foreach (var role in rolePackage.Enhetsregisterroller)
            {
                rolePackages.Add(new RolePackage
                {
                    Id = Guid.NewGuid(),
                    PackageId = uniquePackages[rolePackage.Tilgangspakke],
                    RoleId = uniqueRoles[role],
                    HasAccess = rolePackage.HarTilgang,
                    CanDelegate = rolePackage.Delegerbar,
                });
            }
        }

        var dbItems = await rolePackageService.Get();

        foreach (var item in rolePackages)
        {
            var dbItem = dbItems.FirstOrDefault(x => x.RoleId.ToString() == item.RoleId.ToString() && x.PackageId.ToString() == item.PackageId.ToString());
            item.Id = dbItem != null ? dbItem.Id : Guid.NewGuid();
        }

        result.Add(await IngestData(rolePackageService, rolePackages, new Dictionary<string, List<RolePackage>>(), cancellationToken));

        return result;
    }

    private async Task<List<IngestResult>> IngestAreasAndPackages(CancellationToken cancellationToken = default) 
    {
        var result = new List<IngestResult>();
        var ragnhildResult = await ReadAndSplitJson();
        var ragnhildEngResult = await ReadAndSplitJson("eng");
        var ragnhildNnoResult = await ReadAndSplitJson("nno");

        var areaGroupItems = new Dictionary<string, List<AreaGroup>>
        {
            { "nno", ragnhildNnoResult.AreaGroupItems },
            { "eng", ragnhildEngResult.AreaGroupItems }
        };

        var areaItems = new Dictionary<string, List<Area>>
        {
            { "nno", ragnhildNnoResult.AreaItems },
            { "eng", ragnhildEngResult.AreaItems }
        };

        var packageItems = new Dictionary<string, List<Package>>
        {
            { "nno", ragnhildNnoResult.PackageItems },
            { "eng", ragnhildEngResult.PackageItems }
        };

        result.Add(await IngestData(areaGroupService, ragnhildResult.AreaGroupItems, areaGroupItems, cancellationToken));
        result.Add(await IngestData(areaService, ragnhildResult.AreaItems, areaItems, cancellationToken));
        //// result.Add(await IngestData(packageService, ragnhildResult.PackageItems, packageItems, cancellationToken));
        return result;
    }

    private async Task<(List<AreaGroup> AreaGroupItems, List<Area> AreaItems, List<Package> PackageItems)> ReadAndSplitJson(string language = "")
    {
        Console.WriteLine("RagnhildFactory.Go!!");
        var jsonData = await ReadJsonData("All", language);
        var metaAreaGroups = JsonSerializer.Deserialize<List<MetaAreaGroup>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        List<MetaAreaGroup> metaAreaGroupsList = new List<MetaAreaGroup>();
        List<MetaArea> metaAreaList = new List<MetaArea>();
        List<MetaPackage> metaPackagesList = new List<MetaPackage>();

        foreach (var areaGroup in metaAreaGroups)
        {
            MetaAreaGroup newAreaGroup = new MetaAreaGroup
            {
                Id = areaGroup.Id,
                Name = areaGroup.Name,
                Description = areaGroup.Description,
                Type = areaGroup.Type,
                Urn = areaGroup.Urn,
                Areas = areaGroup.Areas,
            };

            metaAreaGroupsList.Add(newAreaGroup);

            foreach (var area in areaGroup.Areas)
            {
                MetaArea newArea = new MetaArea
                {
                    Id = area.Id,
                    Name = area.Name,
                    Description = area.Description,
                    Icon = area.Icon,
                    AreaGroup = area.AreaGroup,
                    Packages = area.Packages,
                };

                metaAreaList.Add(newArea);

                foreach (var package in area.Packages)
                {
                    MetaPackage newPackage = new MetaPackage
                    {
                        Id = package.Id,
                        Urn = package.Urn,
                        Name = package.Name,
                        Description = package.Description,
                        Area = area.Name,
                    };
                    metaPackagesList.Add(newPackage);
                }
            }
        }

        var areaGroups = new List<AreaGroup>();
        var areas = new List<Area>();
        var packages = new List<Package>();

        foreach (var areaGroup in metaAreaGroupsList)
        {
            areaGroups.Add(new AreaGroup
            {
                Id = areaGroup.Id,
                Name = areaGroup.Name,
                Description = areaGroup.Description,
            });
        }

        foreach (var area in metaAreaList)
        {
            areas.Add(new Area
            {
                Id = area.Id,
                Name = area.Name,
                Description = area.Description,
                IconName = area.Icon,
                GroupId = areaGroups.First(x => x.Name == area.AreaGroup).Id,
            });
        }

        foreach (var package in metaPackagesList)
        {
            packages.Add(new Package
            {
                Id = package.Id,
                Name = package.Name,
                Description = package.Description,
                AreaId = areas.First(x => x.Name == package.Area).Id,
                IsDelegable = true,
                HasResources = true,
                EntityTypeId = Guid.Parse("8c216e2f-afdd-4234-9ba2-691c727bb33d"),
                ProviderId = Guid.Parse("73dfe32a-8f21-465c-9242-40d82e61f320"),
            });
        }

        return (areaGroups, areas, packages);
    }

    private async Task<string> ReadJsonData<T>(string? language = null, CancellationToken cancellationToken = default)
    {
        return await ReadJsonData(typeof(T).Name, language, cancellationToken);
    }

    private async Task<string> ReadJsonData(string baseName, string? language = null, CancellationToken cancellationToken = default)
    {

        string fileName = $"{Config.BasePath}{Path.DirectorySeparatorChar}{baseName}{(string.IsNullOrEmpty(language) ? string.Empty : "_" + language)}.json";
        if (File.Exists(fileName))
        {
            return await File.ReadAllTextAsync(fileName, cancellationToken);
        }

        return "[]";
    }

    private async Task IngestTranslation<T, TService>(TService service, Dictionary<string, List<T>> languageJsonItems, CancellationToken cancellationToken)
         where TService : IDbBasicDataService<T>
    {
        var type = typeof(T);
        foreach (var translatedItems in languageJsonItems)
        {
            var dbItems = await service.Get(options: new RequestOptions() { Language = translatedItems.Key });
            //Console.WriteLine($"Ingest {lang} {type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");
            foreach (var item in translatedItems.Value)
            {
                try
                {
                    var id = GetId(item);
                    if (id == null)
                    {
                        throw new Exception($"Failed to get Id for '{typeof(T).Name}'");
                    }

                    var rowchanges = await service.Repo.UpdateTranslation(id.Value, item, translatedItems.Key);
                    if (rowchanges > 0)
                    {
                        continue;
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to update translation");
                }

                try
                {
                    await service.Repo.CreateTranslation(item, translatedItems.Key);
                }
                catch
                {
                    Console.WriteLine("Failed to create translation");
                }
            }
        }
    }

    private TValue? GetValue<T, TValue>(T item, string propertyName)
    {
        var pt = typeof(T).GetProperty(propertyName);
        if (pt == null)
        {
            return default;
        }

        return (TValue?)pt.GetValue(item) ?? default;
    }

    private Guid? GetId<T>(T item)
    {
        return GetValue<T, Guid?>(item, "Id");
    }

    private bool PropertyComparer<T>(T a, T b)
    {
        foreach (var prop in typeof(T).GetProperties())
        {
            if (prop.PropertyType.Name.ToLower() == "string")
            {
                string? valueA = (string?)prop.GetValue(a);
                string? valueB = (string?)prop.GetValue(b);
                if (string.IsNullOrEmpty(valueA) || string.IsNullOrEmpty(valueB))
                {
                    return false;
                }

                if (!valueA.Equals(valueB))
                {
                    return false;
                }
            }

            if (prop.PropertyType.Name.ToLower() == "guid")
            {
                Guid? valueA = (Guid?)prop.GetValue(a);
                Guid? valueB = (Guid?)prop.GetValue(b);
                if (!valueA.HasValue || !valueB.HasValue)
                {
                    return false;
                }

                if (!valueA.Equals(valueB))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IdComparer<T>(T a, T b)
    {
        try
        {
            var idA = GetId(a);
            var idB = GetId(b);
            if (!idA.HasValue || !idB.HasValue)
            {
                return false;
            }

            if (idA.Value.Equals(idB.Value))
            {
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
