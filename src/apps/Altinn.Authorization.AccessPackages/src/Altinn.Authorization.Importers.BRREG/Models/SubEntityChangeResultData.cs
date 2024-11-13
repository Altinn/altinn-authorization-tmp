using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

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
