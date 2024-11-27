using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// SubEntityChangeResultData
/// </summary>
public class SubEntityChangeResultData
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("oppdaterteUnderenheter")]
    public List<UnitChange> Data { get; set; }
}
