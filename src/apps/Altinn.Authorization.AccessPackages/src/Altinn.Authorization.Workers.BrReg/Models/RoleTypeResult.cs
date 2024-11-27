using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

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
