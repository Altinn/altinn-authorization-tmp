using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// RoleGroup
/// </summary>
public class RoleGroup
{
    /// <summary>
    /// Type
    /// </summary>
    [JsonPropertyName("type")]
    public RoleGroupType Type { get; set; }

    /// <summary>
    /// LastChanged
    /// </summary>
    [JsonPropertyName("sistEndret")]
    public string LastChanged { get; set; }

    /// <summary>
    /// Roles
    /// </summary>
    [JsonPropertyName("roller")]
    public List<UnitRole> Roles { get; set; }
}
