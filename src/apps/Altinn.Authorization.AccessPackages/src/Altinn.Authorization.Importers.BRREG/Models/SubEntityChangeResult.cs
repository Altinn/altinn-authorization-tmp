using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// SubEntityChangeResult
/// </summary>
public class SubEntityChangeResult : BaseResult, IChangeResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public SubEntityChangeResultData Elements { get; set; }
}
