using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// EntityChangeResultData
/// </summary>
public class EntityChangeResultData
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("oppdaterteEnheter")]
    public List<UnitChange> Data { get; set; }
}
