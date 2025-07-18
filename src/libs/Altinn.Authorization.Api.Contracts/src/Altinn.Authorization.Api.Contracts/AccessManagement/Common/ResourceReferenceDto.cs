using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

/// <summary>
/// Resource reference DTO
/// </summary>
public class ResourceReferenceDto
{
    public string ReferenceType { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string? ReferenceSource { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReferenceSourceExternal
{
    Altinn2,
    Altinn3,
    ResourceRegistry
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReferenceTypeExternal
{
    ApplicationId,
    ServiceCode,
    ResourceId,
    Uri
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ResourceTypeExternal
{
    GenericAccessResource,
    MaskinportenSchema,
    AltinnApp,
    Systemresource,
    Default
}