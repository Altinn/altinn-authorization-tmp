using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// RoleResult
/// </summary>
public class RoleResult
{
    /// <summary>
    /// OrgNo
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string OrgNo { get; set; }

    /// <summary>
    /// RoleGroups
    /// </summary>
    [JsonPropertyName("rollegrupper")]
    public List<RoleGroup> RoleGroups { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    [JsonPropertyName("_links")]
    public object Links { get; set; }
}
