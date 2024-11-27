using System.Text.Json.Serialization;

namespace Altinn.Authorization.Workers.BrReg.Models;

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
