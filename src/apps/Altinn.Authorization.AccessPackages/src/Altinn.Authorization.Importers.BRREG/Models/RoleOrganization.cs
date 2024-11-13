using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// RoleOrganization
/// </summary>
public class RoleOrganization
{
    /// <summary>
    /// OrgNo
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string OrgNo { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    [JsonPropertyName("navn")]
    public string[] Name { get; set; }

    /// <summary>
    /// IsDeleted
    /// </summary>
    [JsonPropertyName("erSlettet")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// OrgForm
    /// </summary>
    [JsonPropertyName("organisasjonsform")]
    public UnitForm OrgForm { get; set; }
}