using System.Text.Json;
using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.AccessPackages.Repo.Ingest.RagnhildModel;
using Altinn.AccessMgmt.AccessPackages.Repo.Mock;
using Altinn.AccessMgmt.DbAccess.Data.Contracts;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.Models;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Ingest;

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

        if (Config.Enabled.ContainsKey("providerIngestService") && Config.Enabled["providerIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("providerIngestService"));
            result.Add(await IngestData<Provider, IProviderService>(providerService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("entityTypeIngestService") && Config.Enabled["entityTypeIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("entityTypeIngestService"));
            result.Add(await IngestData<EntityType, IEntityTypeService>(entityTypeService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("entityVariantIngestService") && Config.Enabled["entityVariantIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantIngestService"));
            result.Add(await IngestData<EntityVariant, IEntityVariantService>(entityVariantService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("roleIngestService") && Config.Enabled["roleIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("roleIngestService"));
            result.Add(await IngestData<Role, IRoleService>(roleService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("roleMapIngestService") && Config.Enabled["roleMapIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("roleMapIngestService"));
            result.Add(await IngestData<RoleMap, IRoleMapService>(roleMapService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("areasAndPackagesIngestService") && Config.Enabled["areasAndPackagesIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("areasAndPackagesIngestService"));
            result.AddRange(await IngestAreasAndPackages(cancellationToken));
        }

        if (Config.Enabled.ContainsKey("RolePackagesIngestService") && Config.Enabled["RolePackagesIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("RolePackagesIngestService"));
            result.AddRange(await IngestRolePackages(cancellationToken));
        }

        if (Config.Enabled.ContainsKey("tagGroupIngestService") && Config.Enabled["tagGroupIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("tagGroupIngestService"));
            result.Add(await IngestData<TagGroup, ITagGroupService>(tagGroupService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("tagIngestService") && Config.Enabled["tagIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("tagIngestService"));
            result.Add(await IngestData<Tag, ITagService>(tagService, cancellationToken));
        }

        if (Config.Enabled.ContainsKey("entityVariantRoleIngestService") && Config.Enabled["entityVariantRoleIngestService"])
        {
            a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantRoleIngestService"));
            result.Add(await IngestData<EntityVariantRole, IEntityVariantRoleService>(entityVariantRoleService, cancellationToken));
        }

        try
        {
            await TempRolePackageFix();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return result;
    }

    private async Task TempRolePackageFix()
    {
        var allareas = await areaService.Get();
        var allpackages = await packageService.Get();
        var allroles = await roleService.Get();

        var regnArea = allareas.First(t => t.Name == "Fullmakter for regnskapsfører");
        var reviArea = allareas.First(t => t.Name == "Fullmakter for revisor");
        var bobeArea = allareas.First(t => t.Name == "Fullmakter for konkursbo");

        var regnRole = allroles.First(t => t.Code.ToLower() == "regn");
        var reviRole = allroles.First(t => t.Code.ToLower() == "revi");
        var bobeRole = allroles.First(t => t.Code.ToLower() == "bobe");

        var regnPackages = allpackages.Where(t => t.AreaId == regnArea.Id);
        var reviPackages = allpackages.Where(t => t.AreaId == reviArea.Id);
        var bobePackages = allpackages.Where(t => t.AreaId == bobeArea.Id);

        var rolePackCache = await rolePackageService.Get();

        await MapPackagesToRole(regnRole, regnPackages);
        await MapPackagesToRole(reviRole, reviPackages);
        await MapPackagesToRole(bobeRole, bobePackages);

        async Task MapPackagesToRole(Role role, IEnumerable<Package> packages)
        {
            foreach (var pck in packages)
            {
                if (rolePackCache.Count(t => t.PackageId == pck.Id && t.RoleId == regnRole.Id) > 0)
                {
                    continue;
                }

                try
                {
                    await rolePackageService.Create(new RolePackage()
                    {
                        Id = Guid.NewGuid(),
                        HasAccess = true,
                        CanDelegate = true,
                        PackageId = pck.Id,
                        RoleId = role.Id,
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to map package to role. {pck.Name} => {role.Name}");
                }
            }
        }
    }

    private async Task<IngestResult> IngestData<T, TService>(TService service, CancellationToken cancellationToken)
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

    private async Task<IngestResult> IngestData<T, TService>(TService service, List<T> jsonItems, Dictionary<string, List<T>> languageJsonItems, CancellationToken cancellationToken)
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

    private async Task<List<IngestResult>> IngestRolePackages(CancellationToken cancellationToken = default, string language = "")
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
            try
            {
                uniquePackages.TryAdd(rolePackage.Tilgangspakke, allpackages.First(x => x.Name == rolePackage.Tilgangspakke).Id);
                foreach (var role in rolePackage.Enhetsregisterroller)
                {
                    uniqueRoles.TryAdd(role, allroles.First(x => x.Name.ToLower() == role.ToLower()).Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {rolePackage.Name}");
                Console.WriteLine(ex.Message);
            }
        }

        foreach (var rolePackage in metaRolePackages)
        {
            foreach (var role in rolePackage.Enhetsregisterroller)
            {
                try
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {rolePackage.Name} - {role}");
                    Console.WriteLine(ex.Message);
                }
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
        var ingestResult = new List<IngestResult>();
        var result = await ReadAndSplitAreasAndPackagesJson();
        var resultEng = await ReadAndSplitAreasAndPackagesJson("eng");
        var resultNno = await ReadAndSplitAreasAndPackagesJson("nno");

        var areaGroupItems = new Dictionary<string, List<AreaGroup>>
        {
            { "nno", resultNno.AreaGroupItems },
            { "eng", resultEng.AreaGroupItems }
        };

        var areaItems = new Dictionary<string, List<Area>>
        {
            { "nno", resultNno.AreaItems },
            { "eng", resultEng.AreaItems }
        };

        var packageItems = new Dictionary<string, List<Package>>
        {
            { "nno", resultNno.PackageItems },
            { "eng", resultEng.PackageItems }
        };

        ingestResult.Add(await IngestData(areaGroupService, result.AreaGroupItems, areaGroupItems, cancellationToken));
        ingestResult.Add(await IngestData(areaService, result.AreaItems, areaItems, cancellationToken));
        ingestResult.Add(await IngestData(packageService, result.PackageItems, packageItems, cancellationToken));
        return ingestResult;
    }

    private async Task<(List<AreaGroup> AreaGroupItems, List<Area> AreaItems, List<Package> PackageItems)> ReadAndSplitAreasAndPackagesJson2(string language = "")
    {
        List<AreaGroup> areaGroups = [];
        List<Area> areas = [];
        List<Package> packages = [];

        // TODO: Check if this should be in json data
        var entityType = (await entityTypeService.Get(t => t.Name, "Organisasjon")).First() ?? throw new Exception("Unable to fint 'Organisasjon' entityType");

        // TODO: Check if this should be in json data
        var provider = (await providerService.Get(t => t.Name, "Digdir")).First() ?? throw new Exception("Unable to fint 'Digdir' provider");

        // TODO: Rename file to AreaAndPackages.json
        var jsonData = await ReadJsonData("All", language);
        List<MetaAreaGroup> metaAreaGroups = [.. JsonSerializer.Deserialize<List<MetaAreaGroup>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })];

        foreach (var meta in metaAreaGroups)
        {
            areaGroups.Add(new AreaGroup()
            {
                Id = meta.Id,
                Name = meta.Name,
                Description = meta.Description,

                // TODO: Extend model to store EntityTypeId with refrence to EntityType
                // EntityTypeId = (await entityTypeService.Get(t => t.Name, meta.Type)).First(),

                // TODO: If needed; extend model to store Urn
                // Urn = meta.Urn,
            });

            if (meta.Areas != null)
            {
                foreach (var area in meta.Areas)
                {
                    areas.Add(new Area()
                    {
                        Id = area.Id,
                        Name = area.Name,
                        Description = area.Description,
                        GroupId = meta.Id,
                        IconName = area.Icon,

                        // TODO: If needed; extend model to store Urn
                        // Urn = area.Urn,
                    });

                    if (area.Packages != null)
                    {
                        foreach (var package in area.Packages)
                        {
                            packages.Add(new Package()
                            {
                                Id = package.Id,
                                Name = package.Name,
                                Description = package.Description,
                                AreaId = area.Id,
                                EntityTypeId = entityType.Id,
                                ProviderId = provider.Id,
                                IsDelegable = true,
                                HasResources = true,

                                // TODO: Extend model to store Urn
                                // Urn = package.Urn,
                            });
                        }
                    }
                }
            }
        }

        return (areaGroups, areas, packages);
    }

    private async Task<(List<AreaGroup> AreaGroupItems, List<Area> AreaItems, List<Package> PackageItems)> ReadAndSplitAreasAndPackagesJson(string language = "")
    {
        var jsonData = await ReadJsonData("All", language);
        List<MetaAreaGroup> metaAreaGroups = [.. JsonSerializer.Deserialize<List<MetaAreaGroup>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })];
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
            //// Console.WriteLine($"Ingest {lang} {type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");
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
