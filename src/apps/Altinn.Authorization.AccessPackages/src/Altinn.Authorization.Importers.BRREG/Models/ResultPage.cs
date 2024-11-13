using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// ResultPage
/// </summary>
public class ResultPage
{
    /// <summary>
    /// Size
    /// </summary>
    [JsonPropertyName("size")]
    public int Size { get; set; }

    /// <summary>
    /// TotalElements
    /// </summary>
    [JsonPropertyName("totalElements")]
    public int TotalElements { get; set; }

    /// <summary>
    /// TotalPages
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    /// <summary>
    /// CurrentPage
    /// </summary>
    [JsonPropertyName("number")]
    public int CurrentPage { get; set; }
}