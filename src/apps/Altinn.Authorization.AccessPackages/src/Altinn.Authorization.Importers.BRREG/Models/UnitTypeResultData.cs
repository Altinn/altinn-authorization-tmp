using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// UnitTypeResultData
/// </summary>
public class UnitTypeResultData
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("organisasjonsformer")]
    public List<UnitType> Data { get; set; }
}
