using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

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
