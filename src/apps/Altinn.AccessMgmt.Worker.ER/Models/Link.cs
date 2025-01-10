using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

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
