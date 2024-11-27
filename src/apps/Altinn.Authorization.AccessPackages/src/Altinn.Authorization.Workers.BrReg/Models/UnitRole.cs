using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// UnitRole
/// </summary>
public class UnitRole
{
    /// <summary>
    /// Type
    /// </summary>
    [JsonPropertyName("type")]
    public RoleType Type { get; set; }

    /// <summary>
    /// Person
    /// </summary>
    [JsonPropertyName("person")]
    public RolePerson Person { get; set; }

    /// <summary>
    /// Organization
    /// </summary>
    [JsonPropertyName("enhet")]
    public RoleOrganization Organization { get; set; }

    /// <summary>
    /// Deactivated
    /// </summary>
    [JsonPropertyName("fratraadt")]
    public bool Deactivated { get; set; }

    /// <summary>
    /// Order
    /// </summary>
    [JsonPropertyName("rekkefolge")]
    public int Order { get; set; }
}
