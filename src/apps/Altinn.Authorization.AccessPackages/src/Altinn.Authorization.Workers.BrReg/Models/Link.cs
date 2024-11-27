using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

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
