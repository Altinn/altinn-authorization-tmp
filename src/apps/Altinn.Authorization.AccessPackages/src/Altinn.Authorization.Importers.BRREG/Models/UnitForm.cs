using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// UnitForm
/// </summary>
public class UnitForm
{
    /// <summary>
    /// Code
    /// </summary>
    [JsonPropertyName("kode")]
    public string Code { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [JsonPropertyName("beskrivelse")]
    public string Description { get; set; }
}
