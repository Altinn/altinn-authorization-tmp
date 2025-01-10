using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

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
