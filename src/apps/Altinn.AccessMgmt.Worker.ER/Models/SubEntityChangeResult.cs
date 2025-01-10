using System.Text.Json.Serialization;

namespace Altinn.AccessMgmt.Worker.ER.Models;

/// <summary>
/// SubEntityChangeResult
/// </summary>
public class SubEntityChangeResult : BaseResult, IChangeResult
{
    /// <summary>
    /// Elements
    /// </summary>
    [JsonPropertyName("_embedded")]
    public SubEntityChangeResultData Elements { get; set; }
}
