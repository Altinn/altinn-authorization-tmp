using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// RoleChange
/// </summary>
public class RoleChange
{
    /// <summary>
    /// Version
    /// </summary>
    [JsonPropertyName("specversion")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// ChangeId
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    /// DbAccessSource
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Type
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Time
    /// </summary>
    [JsonPropertyName("time")]
    public DateTime Time { get; set; }

    /// <summary>
    /// Data
    /// </summary>
    [JsonPropertyName("data")]
    public RoleChangeResultData Data { get; set; } = new RoleChangeResultData();
}
