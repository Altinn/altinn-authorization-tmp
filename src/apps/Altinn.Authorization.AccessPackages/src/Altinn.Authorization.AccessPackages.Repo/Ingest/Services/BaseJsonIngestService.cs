using System.Text.Json;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Models;
using Altinn.Authorization.AccessPackages.DbAccess.Ingest.Models;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.AccessPackages.Repo.Ingest.Services;

/// <summary>
/// Base Json Ingest Service
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TRepo"></typeparam>
public class BaseJsonIngestService<T, TRepo>
     where TRepo : IDbBasicDataService<T>
{
    /// <summary>
    /// Dataservice to ingest data
    /// </summary>
    public TRepo DataService { get; }

    /// <summary>
    /// Configuration
    /// </summary>
    public JsonIngestConfig Config { get; set; }

    /// <summary>
    /// JsonIngestMeters
    /// </summary>
    public JsonIngestMeters Meters { get; set; }

    /// <summary>
    /// Type to ingest
    /// </summary>
    private Type Type { get { return typeof(T); } }

    /// <summary>
    /// Hints if translations are available
    /// </summary>
    public bool LoadTranslations { get; set; } = false;

    /// <summary>
    /// Base Json Ingest Service
    /// </summary>
    /// <param name="service">Data service</param>
    /// <param name="config">JsonIngestConfig</param>
    public BaseJsonIngestService(TRepo service, IOptions<JsonIngestConfig> config, JsonIngestMeters meters)
    {
        DataService = service;
        Config = config.Value;
        Meters = meters;
    }

    /// <summary>
    /// Ingest data
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    public async Task<IngestResult> IngestData(CancellationToken cancellationToken)
    {
        //using var a = RepoTelemetry.StartDbAccessActivity<T>("IngestData");
        var jsonData = await ReadJsonData(cancellationToken: cancellationToken);
        if (jsonData == "[]")
        {
            return new IngestResult(Type) { Success = false };
        }

        var jsonItems = JsonSerializer.Deserialize<List<T>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        if (jsonItems == null)
        {
            return new IngestResult(Type) { Success = false };
        }

        var dbItems = await DataService.Get();

        //Meters.SetIngestValues(Type.Name, jsonItems.Count, dbItems.Count());

        Console.WriteLine($"Ingest {Type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");

        if (dbItems == null || !dbItems.Any())
        { 
            foreach (var item in jsonItems)
            {
                await DataService.Create(item);
            }
        }
        else
        {
            foreach (var item in jsonItems)
            {
                if (dbItems.Count(t => IdComparer(item, t)) == 0)
                {
                    await DataService.Create(item);
                    continue;
                }

                if (dbItems.Count(t => PropertyComparer(item, t)) == 0)
                {
                    var id = GetId(item);
                    if (id.HasValue)
                    {
                        await DataService.Update(id.Value, item);
                    }
                }
            }
        }

        if (LoadTranslations)
        {
            await IngestTranslation(cancellationToken);
        }

        return new IngestResult(Type) { Success = true };
    }

    /// <summary>
    /// Read Json data file
    /// </summary>
    /// <param name="language">Language code (e.g. nob, nno, eng)</param>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    private async Task<string> ReadJsonData(string? language = null, CancellationToken cancellationToken = default)
    {
        
        string fileName = $"{Config.BasePath}{Path.DirectorySeparatorChar}{Type.Name}{(string.IsNullOrEmpty(language) ? string.Empty : "_" + language)}.json";
        if (File.Exists(fileName))
        {
            return await File.ReadAllTextAsync(fileName, cancellationToken);
        }
        //else
        //{
        //    Console.WriteLine(Directory.GetCurrentDirectory());
        //    Console.WriteLine($"File not found: '{fileName}'");
        //}

        return "[]";
    }

    /// <summary>
    /// Ingest translations from Json files
    /// </summary>
    /// <param name="cancellationToken">CancellationToken</param>
    /// <returns></returns>
    private async Task IngestTranslation(CancellationToken cancellationToken)
    {
        foreach (var lang in Config.Languages)
        {
            var jsonData = await ReadJsonData(language: lang, cancellationToken: cancellationToken);
            if (jsonData == "[]")
            {
                continue;
            }

            var jsonItems = JsonSerializer.Deserialize<List<T>>(jsonData, options: new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            if (jsonItems == null)
            {
                return;
            }

            var dbItems = await DataService.Get(options: new RequestOptions() { Language = lang });
            Console.WriteLine($"Ingest {lang} {Type.Name} Json:{jsonItems.Count} Db:{dbItems.Count()}");
            foreach (var item in jsonItems)
            {
                if (dbItems == null || !dbItems.Any())
                {
                    await DataService.Repo.CreateTranslation(item, lang);
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

                        await DataService.Repo.UpdateTranslation(id.Value, item, lang);
                        continue;
                    }
                    catch
                    {
                        Console.WriteLine("Failed to update translation");
                    }

                    try
                    {
                        await DataService.Repo.CreateTranslation(item, lang);
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

    private TValue? GetValue<TValue>(T item, string propertyName)
    {
        var pt = Type.GetProperty(propertyName);
        if (pt == null)
        {
            return default;
        }

        return (TValue?)pt.GetValue(item) ?? default;
    }

    private Guid? GetId(T item)
    {
        return GetValue<Guid?>(item, "Id");
    }

    private bool PropertyComparer(T a, T b)
    {
        foreach (var prop in Type.GetProperties())
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

    private bool IdComparer(T a, T b)
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
