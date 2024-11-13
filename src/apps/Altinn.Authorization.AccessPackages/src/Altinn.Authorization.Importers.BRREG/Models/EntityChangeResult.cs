using System.Text.Json.Serialization;

namespace Altinn.Authorization.Importers.BRREG;

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