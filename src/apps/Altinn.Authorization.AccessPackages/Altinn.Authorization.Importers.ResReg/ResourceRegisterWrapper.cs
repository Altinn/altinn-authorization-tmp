using Altinn.Authorization.Importers.ResReg.Models;
using System.Net.Http.Json;

namespace Altinn.Authorization.Importers.ResReg;

/// <summary>
/// ResourceRegister Wrapper
/// </summary>
public class ResourceRegisterWrapper
{
    public string BaseUrl { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ResourceRegisterWrapper()
    {
        BaseUrl = "https://platform.altinn.no";
    }

    public async Task<List<RawResource>> GetResources()
    {
        HttpClient client = new HttpClient();
        return await client.GetFromJsonAsync<List<RawResource>>($"{BaseUrl}/resourceregistry/api/v1/resource/resourcelist") ?? new List<RawResource>();
    }
}
