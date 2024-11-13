using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// BaseResult
/// </summary>
public class BaseResult
{
    /// <summary>
    /// Links
    /// </summary>
    [JsonPropertyName("_links")]
    public ResultLinks Links { get; set; }

    /// <summary>
    /// Page
    /// </summary>
    [JsonPropertyName("page")]
    public ResultPage Page { get; set; }
}
