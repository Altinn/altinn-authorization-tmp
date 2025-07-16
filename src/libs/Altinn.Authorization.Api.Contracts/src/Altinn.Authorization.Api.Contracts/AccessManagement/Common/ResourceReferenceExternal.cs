using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

/// <summary>
/// Model representation of the resource reference part of the ServiceResource model
/// </summary>
public class ResourceReferenceExternal
{
    /// <summary>
    /// The source the reference identifier points to
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReferenceSourceExternal? ReferenceSource { get; set; }

    /// <summary>
    /// The reference identifier
    /// </summary>
    public string Reference { get; set; }

    /// <summary>
    /// The reference type
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReferenceTypeExternal? ReferenceType { get; set; }
}
