using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// UnitTypeResult
/// </summary>
public class UnitTypeResult : BaseResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public UnitTypeResultData Elements { get; set; }
}
