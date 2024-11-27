using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// UnitChange
/// </summary>
public class UnitChange
{
    /// <summary>
    /// ChangeId
    /// </summary>
    [JsonPropertyName("oppdateringsid")]
    public int ChangeId { get; set; }

    /// <summary>
    /// Date
    /// </summary>
    [JsonPropertyName("dato")]
    public string Date { get; set; }

    /// <summary>
    /// OrgNo
    /// </summary>
    [JsonPropertyName("organisasjonsnummer")]
    public string OrgNo { get; set; }

    /// <summary>
    /// ChangeType
    /// </summary>
    [JsonPropertyName("endringstype")]
    public string ChangeType { get; set; }

    /// <summary>
    /// Links
    /// </summary>
    [JsonPropertyName("_links")]
    public UnitChangeLinks Links { get; set; }
}

/// <summary>
/// UnitChangeLinks
/// </summary>
public class UnitChangeLinks
{
    /// <summary>
    /// SubUnit
    /// </summary>
    [JsonPropertyName("underenhet")]
    public Link SubUnit { get; set; }

    /// <summary>
    /// Unit
    /// </summary>
    [JsonPropertyName("enhet")]
    public Link Unit { get; set; }

    /// <summary>
    /// GetSelfLink
    /// </summary>
    /// <returns></returns>
    public string GetSelfLink()
    {
        if (SubUnit != null)
        {
            return SubUnit.Href;
        }

        if (Unit != null)
        {
            return Unit.Href;
        }

        return string.Empty;
    }
}
