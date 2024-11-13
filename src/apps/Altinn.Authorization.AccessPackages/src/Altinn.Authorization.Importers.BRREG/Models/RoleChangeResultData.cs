using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

/// <summary>
/// RoleChangeResultData
/// </summary>
public class RoleChangeResultData
{
    /// <summary>
    /// OrgNo
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string OrgNo { get; set; } = string.Empty;
}
