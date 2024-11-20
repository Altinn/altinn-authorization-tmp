using System.Text.Json;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
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

        a?.AddEvent(new System.Diagnostics.ActivityEvent("areaGroupIngestService"));
        result.Add(await IngestData<AreaGroup, IAreaGroupService>(areaGroupService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("areaIngestService"));
        result.Add(await IngestData<Area, IAreaService>(areaService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("providerIngestService"));
        result.Add(await IngestData<Provider, IProviderService>(providerService, cancellationToken));

        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityTypeIngestService"));
        result.Add(await IngestData<EntityType, IEntityTypeService>(entityTypeService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("entityVariantIngestService"));
        result.Add(await IngestData<EntityVariant, IEntityVariantService>(entityVariantService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("packageIngestService"));
        result.Add(await IngestData<Package, IPackageService>(packageService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleIngestService"));
        result.Add(await IngestData<Role, IRoleService>(roleService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("roleMapIngestService"));
        result.Add(await IngestData<RoleMap, IRoleMapService>(roleMapService, cancellationToken));
        
        a?.AddEvent(new System.Diagnostics.ActivityEvent("rolePackageIngestService"));
        result.Add(await IngestData<RolePackage, IRolePackageService>(rolePackageService, cancellationToken));
        
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

        var dbItems = await service.Get();

        Console.WriteLine($"Ingest {type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");

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

        await IngestTranslation<T, TService>(service, cancellationToken);
        
        return new IngestResult(type) { Success = true };
    }
    
    private async Task<string> ReadJsonData<T>(string? language = null, CancellationToken cancellationToken = default)
    {

        string fileName = $"{Config.BasePath}{Path.DirectorySeparatorChar}{typeof(T).Name}{(string.IsNullOrEmpty(language) ? string.Empty : "_" + language)}.json";
        if (File.Exists(fileName))
        {
            return await File.ReadAllTextAsync(fileName, cancellationToken);
        }

        return "[]";
    }

    private async Task IngestTranslation<T, TService>(TService service, CancellationToken cancellationToken)
         where TService : IDbBasicDataService<T>
    {
        var type = typeof(T);
        foreach (var lang in Config.Languages)
        {
            var jsonData = await ReadJsonData<T>(language: lang, cancellationToken: cancellationToken);
            if (jsonData == "[]")
            {
                continue;
            }

            var jsonItems = JsonSerializer.Deserialize<List<T>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (jsonItems == null)
            {
                return;
            }

            var dbItems = await service.Get(options: new RequestOptions() { Language = lang });
            Console.WriteLine($"Ingest {lang} {type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");
            foreach (var item in jsonItems)
            {
                if (dbItems == null || !dbItems.Any())
                {
                    await service.Repo.CreateTranslation(item, lang);
                }
                else
                {
                    // Console.WriteLine(JsonSerializer.Serialize(item));

                    // TODO : Make it better .... 
                    try
                    {
                        var id = GetId(item);
                        if (id == null)
                        {
                            throw new Exception($"Failed to get Id for '{typeof(T).Name}'");
                        }

                        await service.Repo.UpdateTranslation(id.Value, item, lang);
                        continue;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to update translation");
                    }

                    try
                    {
                        await service.Repo.CreateTranslation(item, lang);
                        continue;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to create translation");
                    }
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
