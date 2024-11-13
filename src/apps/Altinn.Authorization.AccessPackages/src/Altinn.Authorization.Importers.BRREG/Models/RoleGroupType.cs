using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// RoleGroupType
/// </summary>
public class RoleGroupType
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

    /// <summary>
    /// Links
    /// </summary>
    [JsonPropertyName("_links")]
    public object Links { get; set; } // _self
}
