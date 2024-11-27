using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

/// <summary>
/// RoleTypeResultData
/// </summary>
public class RoleTypeResultData
{
    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("rolletyper")]
    public List<RoleType> Data { get; set; }
}
