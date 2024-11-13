using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// ResultLinks
/// </summary>
public class ResultLinks
{
    /// <summary>
    /// First
    /// </summary>
    [JsonPropertyName("first")]
    public Link First { get; set; }

    /// <summary>
    /// Self
    /// </summary>
    [JsonPropertyName("self")]
    public Link Self { get; set; }

    /// <summary>
    /// Next
    /// </summary>
    [JsonPropertyName("next")]
    public Link Next { get; set; }

    /// <summary>
    /// Last
    /// </summary>
    [JsonPropertyName("last")]
    public Link Last { get; set; }
}
