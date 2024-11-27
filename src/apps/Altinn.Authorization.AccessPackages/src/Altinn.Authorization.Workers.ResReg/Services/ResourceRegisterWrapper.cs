using System.Net.Http.Json;
using Altinn.Authorization.Importers.ResReg.Models;

namespace Altinn.Authorization.Importers.ResReg.Services;

/// <summary>
/// ResourceRegister Wrapper
/// </summary>
public class ResourceRegisterWrapper
{
    /// <summary>
    /// BaseUrl
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    public ResourceRegisterWrapper()
    {
        BaseUrl = "https://platform.altinn.no";
    }

    /// <summary>
    /// GetResources
    /// </summary>
    /// <returns></returns>
    public async Task<List<RawResource>> GetResources()
    {
        /*
         /resource/{id}/policy/subjects
        /resource/bysubjects
         */

        HttpClient client = new HttpClient();
        return await client.GetFromJsonAsync<List<RawResource>>($"{BaseUrl}/resourceregistry/api/v1/resource/resourcelist") ?? [];
    }
}
