using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// UnitTypeResult
/// </summary>
public class UnitTypeResult : BaseResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public UnitTypeResultData Elements { get; set; }
}
