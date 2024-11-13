using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// Unit
/// </summary>
public class Unit
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
    public string Name { get; set; }

    /// <summary>
    /// ParentOrgNo
    /// </summary>
    [JsonPropertyName("overordnetEnhet")]
    public string? ParentOrgNo { get; set; }

    /// <summary>
    /// OrgForm
    /// </summary>
    [JsonPropertyName("organisasjonsform")]
    public UnitForm OrgForm { get; set; }
}
