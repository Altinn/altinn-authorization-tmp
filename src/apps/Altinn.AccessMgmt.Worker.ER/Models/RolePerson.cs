using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// RolePerson
/// </summary>
public class RolePerson
{
    /// <summary>
    /// Birthdate
    /// </summary>
    [JsonPropertyName("fodselsdato")]
    public string Birthdate { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonPropertyName("navn")]
    public PersonName Name { get; set; }

    /// <summary>
    /// IsDead
    /// </summary>
    [JsonPropertyName("erDoed")]
    public bool IsDead { get; set; }
}
