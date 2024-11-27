using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

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
