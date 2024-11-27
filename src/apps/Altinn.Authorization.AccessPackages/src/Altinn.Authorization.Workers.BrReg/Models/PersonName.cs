using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// PersonName
/// </summary>
public class PersonName
{
    /// <summary>
    /// Firstname
    /// </summary>
    [JsonPropertyName("fornavn")]
    public string Firstname { get; set; }

    /// <summary>
    /// Middlename
    /// </summary>
    [JsonPropertyName("mellomnavn")]
    public string Middlename { get; set; }

    /// <summary>
    /// Lastname
    /// </summary>
    [JsonPropertyName("etternavn")]
    public string Lastname { get; set; }
}
