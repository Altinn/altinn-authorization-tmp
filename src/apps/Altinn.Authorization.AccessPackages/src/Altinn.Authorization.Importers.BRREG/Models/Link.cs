using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// Link
/// </summary>
public class Link
{
    /// <summary>
    /// Href
    /// </summary>
    [JsonPropertyName("Href")]
    public string Href { get; set; }
}
