using System.Text.Json.Serialization;

namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Access Package Metadata
/// </summary>
public class AccessPackageMetadataModel
{
    /// <summary>
    /// Name of Access Package
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
}
