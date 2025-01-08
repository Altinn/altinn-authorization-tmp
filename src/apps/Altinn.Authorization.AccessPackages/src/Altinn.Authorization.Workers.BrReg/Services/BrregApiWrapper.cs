using System;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.Authorization.Workers.BrReg.Models;

namespace Altinn.Authorization.Workers.BrReg.Services;

/// <summary>
/// Wrapper for Brønnøysund Register API
/// </summary>
public class BrregApiWrapper
{
    #region Tools
    private static async Task WriteCacheFile(string shortName, string content)
    {
        await File.WriteAllTextAsync(shortName, content);
    }

    private static async Task<List<T>?> GetCacheFile<T>(string shortName, CancellationToken cancellationToken = default)
    {
        if (File.Exists(shortName))
        {
            var res = JsonSerializer.Deserialize<List<T>>(await File.ReadAllTextAsync(shortName, cancellationToken));
            return res ?? throw new Exception("Empty stream result");
        }

        return null;
    }

    private async Task<List<T>> GetHttpStream<T>(string shortName, string url, string? acceptHeader = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient();
            if (acceptHeader != null)
            {
                client.DefaultRequestHeaders.Add("Accept", acceptHeader);
            }

            using var stream = await client.GetStreamAsync(url, cancellationToken);
            using var decompress = new GZipStream(stream, CompressionMode.Decompress);
            
            var res = JsonSerializer.Deserialize<List<T>>(new StreamReader(decompress).BaseStream);

            //// TODO: Fix FileCache for all types
            if (typeof(T) != typeof(RoleResult))
            {
                try
                {
                    await WriteCacheFile(shortName, JsonSerializer.Serialize(res));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to write result to cache for '{typeof(T).Name}'. " + ex.Message);
                }
            }

            return res ?? throw new Exception("Empty stream result");
        }
        catch (Exception ex)
        {
            throw new Exception("Failed while downloading stream", ex);
        }
    }

    /// <summary>
    /// PagedUrl
    /// </summary>
    internal class PagedUrl
    {
        /// <summary>
        /// BaseUrl
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// PageSize
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// CurrentPage
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// LastPage
        /// </summary>
        public int LastPage { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUrl">Baseurl</param>
        /// <param name="pageSize">PageSize</param>
        /// <param name="changeId">ChangeId</param>
        /// <param name="changeDate">ChangeDate</param>
        public PagedUrl(string baseUrl, int pageSize = 1000, int? changeId = null, DateTime? changeDate = null)
        {
            CurrentPage = -1;
            LastPage = 1;
            PageSize = pageSize;
            if (changeId.HasValue)
            {
                BaseUrl = $"{baseUrl}?oppdateringsid={changeId.Value}";
            }

            if (changeDate.HasValue)
            {
                BaseUrl = $"{baseUrl}?dato={changeDate.Value:yyyy-MM-ddTHH:mm:ss.fffZ}";
            }

            if (string.IsNullOrEmpty(BaseUrl))
            {
                throw new Exception("BaseUrl missing");
            }
        }

        /// <summary>
        /// Get data
        /// </summary>
        /// <typeparam name="T">Type to convert data to</typeparam>
        /// <returns></returns>
        public async Task<List<T>> GetData<T>()
            where T : IChangeResult
        {
            using var client = new HttpClient();
            var result = new List<T>();
            while (LastPage > CurrentPage + 1)
            {
                string url = $"{BaseUrl}&size={PageSize}&page={CurrentPage}";
                Console.WriteLine(url);
                var res = await client.GetFromJsonAsync<T>(url);
                if (res != null)
                {
                    result.Add(res);
                    LastPage = res.Page.TotalPages;
                    CurrentPage++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
    #endregion

    #region Unit

    /// <summary>
    /// Get all units
    /// </summary>
    /// <returns></returns>
    public async Task<List<Unit>> GetAllUnits()
    {
        var cacheResult = await GetCacheFile<Unit>("allunits");
        if (cacheResult != null)
        {
            return cacheResult;
        }

        return await GetHttpStream<Unit>("allunits", "https://data.brreg.no/enhetsregisteret/api/enheter/lastned", "application/vnd.brreg.enhetsregisteret.enhet.v2+gzip;charset=UTF-8");
    }

    /// <summary>
    /// Gets a single unit
    /// </summary>
    /// <param name="orgNo">Organisasjonsnummer</param>
    /// <returns></returns>
    public async Task<Unit?> GetUnit(string orgNo)
    {
        var url = $"https://data.brreg.no/enhetsregisteret/api/enheter/{orgNo}";
        try
        {
            using var client = new HttpClient();
            var res = await client.GetFromJsonAsync<Unit>(url);
            if (res == null)
            {
                return null;
            }
            else
            {
                return res ?? null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unit not found '{orgNo}': {ex.Message}");
            throw new Exception($"Http failed '{url}'", ex);
        }
    }

    /// <summary>
    /// Gets changes for units by id or date
    /// </summary>
    /// <param name="pageSize">PageSize</param>
    /// <param name="changeId">Last changeId</param>
    /// <param name="sinceDate">Last sync date</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Only specify afterChangeId OR sinceDate</exception>
    public async Task<List<UnitChange>> GetUnitChanges(int pageSize = 1000, int? changeId = null, DateTime? sinceDate = null)
    {
        if (changeId.HasValue && sinceDate.HasValue)
        {
            throw new ArgumentException("Only specify afterChangeId OR sinceDate");
        }

        var url = new PagedUrl("https://data.brreg.no/enhetsregisteret/api/oppdateringer/enheter", pageSize: pageSize, changeId: changeId, changeDate: sinceDate);
        var changeResults = await url.GetData<EntityChangeResult>();
        if (changeResults == null)
        {
            return [];
        }

        try
        {
            return changeResults.SelectMany(t => t.Elements.Data).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return [];
        }
    }
    #endregion

    #region SubUnit

    /// <summary>
    /// Gets all subunits
    /// </summary>
    /// <returns></returns>
    public async Task<List<Unit>> GetAllSubUnits()
    {
        return await GetHttpStream<Unit>("allsubunits", "https://data.brreg.no/enhetsregisteret/api/underenheter/lastned", "application/vnd.brreg.enhetsregisteret.underenhet.v2+gzip;charset=UTF-8");
    }

    /// <summary>
    /// Gets single subunit
    /// </summary>
    /// <param name="orgNo">Organisasjonsnummer</param>
    /// <returns></returns>
    public async Task<Unit?> GetSubUnit(string orgNo)
    {
        string url = $"https://data.brreg.no/enhetsregisteret/api/underenheter/{orgNo}";
        try
        {
            using var client = new HttpClient();
            var res = await client.GetFromJsonAsync<Unit>(url);
            return res;
        }
        catch (Exception ex)
        {
            throw new Exception($"Http failed '{url}'", ex);
        }
    }

    /// <summary>
    /// Gets changes for subunits by id or date
    /// </summary>
    /// <param name="pageSize">PageSize</param>
    /// <param name="changeId">Last changeId</param>
    /// <param name="sinceDate">Last sync date</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Only specify afterChangeId OR sinceDate</exception>
    public async Task<List<UnitChange>> GetSubUnitChanges(int pageSize = 1000, int? changeId = null, DateTime? sinceDate = null)
    {
        if (changeId.HasValue && sinceDate.HasValue)
        {
            throw new ArgumentException("Only specify afterChangeId OR sinceDate");
        }

        var url = new PagedUrl("https://data.brreg.no/enhetsregisteret/api/oppdateringer/underenheter", pageSize: pageSize, changeId: changeId, changeDate: sinceDate);
        var changeResults = await url.GetData<SubEntityChangeResult>();
        try
        {
            return changeResults.SelectMany(t => t.Elements.Data).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return [];
        }
    }

    #endregion

    #region Roles

    /// <summary>
    /// Gets all roles for all units
    /// </summary>
    /// <returns></returns>
    public async Task<List<RoleResult>> GetAllUnitRoles()
    {
        var cacheResult = await GetCacheFile<RoleResult>("allroles");
        if (cacheResult != null)
        {
            return cacheResult;
        }

        return await GetHttpStream<RoleResult>("allroles", "https://data.brreg.no/enhetsregisteret/api/roller/totalbestand", "application/vnd.brreg.enhetsregisteret.underenhet.v2+gzip;charset=UTF-8");
    }

    /// <summary>
    /// Gets roles for a single unit
    /// </summary>
    /// <param name="orgNo">Organisasjonsnummer</param>
    /// <returns></returns>
    public async Task<RoleResult?> GetUnitRoles(string orgNo)
    {
        using var client = new HttpClient();
        return await client.GetFromJsonAsync<RoleResult>($"https://data.brreg.no/enhetsregisteret/api/enheter/{orgNo}/roller");
    }

    /// <summary>
    /// Gets role changes for all units after changeId
    /// </summary>
    /// <param name="afterChangeId">Last changeId</param>
    /// <returns></returns>
    public async Task<List<RoleChange>> GetAllRoleChanges(int afterChangeId)
    {
        using var client = new HttpClient();
        var changes = new List<RoleChange>();

        int resultCount = 1;
        while (resultCount > 0)
        {
            string urlPath = $"https://data.brreg.no/enhetsregisteret/api/oppdateringer/roller?afterId={afterChangeId}";
            var res = await client.GetFromJsonAsync<List<RoleChange>>(urlPath);
            if (res != null && res.Any())
            {
                changes.AddRange(res);
                resultCount = res.Count;
                afterChangeId = changes.Max(t => t.Id);
            }
            else
            {
                break;
            }
        }

        return changes;
    }

    /// <summary>
    /// Gets role changes for all units after given date
    /// </summary>
    /// <param name="sinceDate">Last sync date</param>
    /// <returns></returns>
    public async Task<List<RoleChange>> GetAllRoleChanges(DateTime sinceDate)
    {
        using var client = new HttpClient();
        var changes = new List<RoleChange>();

        int resultCount = 1;
        while (resultCount > 0)
        {
            string urlPath = $"https://data.brreg.no/enhetsregisteret/api/oppdateringer/roller?afterTime={sinceDate:yyyy-MM-ddTHH:mm:ss.fffZ}";
            var res = await client.GetFromJsonAsync<List<RoleChange>>(urlPath);
            if (res != null && res.Any())
            {
                changes.AddRange(res);
                resultCount = res.Count;
                sinceDate = changes.Max(t => t.Time);
            }
            else
            {
                break;
            }
        }

        return changes;
    }
    #endregion

    #region Types

    /// <summary>
    /// Gets all unit forms (Organisasjonsformer)
    /// </summary>
    /// <returns></returns>
    public async Task<List<UnitType>> GetUnitForms()
    {
        using var client = new HttpClient();
        var result = new List<UnitType>();
        var data = await client.GetFromJsonAsync<UnitTypeResult>("https://data.brreg.no/enhetsregisteret/api/organisasjonsformer");
        if (data != null)
        {
            result.AddRange(data.Elements.Data);
        }

        return result;
    }

    /// <summary>
    /// Gets all roletypes (Rolletyper)
    /// </summary>
    /// <returns></returns>
    public async Task<List<RoleType>> GetEntityRoleTypes()
    {
        using var client = new HttpClient();
        var result = new List<RoleType>();
        var data = await client.GetFromJsonAsync<RoleTypeResult>("https://data.brreg.no/enhetsregisteret/api/roller/rolletyper");
        if (data != null)
        {
            result.AddRange(data.Elements.Data);
        }

        return result;
    }
    #endregion
}
