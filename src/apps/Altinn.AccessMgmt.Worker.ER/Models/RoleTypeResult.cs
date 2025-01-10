using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// RoleTypeResult
/// </summary>
public class RoleTypeResult : BaseResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public RoleTypeResultData Elements { get; set; }
}
