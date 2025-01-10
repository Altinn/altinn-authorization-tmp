using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// EntityChangeResult
/// </summary>
public class EntityChangeResult : BaseResult, IChangeResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public EntityChangeResultData Elements { get; set; }
}
