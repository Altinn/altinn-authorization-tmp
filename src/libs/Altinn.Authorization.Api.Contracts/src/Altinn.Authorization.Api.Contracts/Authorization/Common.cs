using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Attribute match DTO
/// </summary>
public class AttributeMatchDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeMatchTypeDto Type { get; set; } = AttributeMatchTypeDto.Equals;
    
    public string? DataType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AttributeMatchTypeDto
{
    Equals,
    Contains,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum DecisionDto
{
    Permit,
    Deny,
    Indeterminate,
    NotApplicable
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PolicyStatusDto
{
    Active,
    Inactive,
    Draft,
    Deprecated
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizationRequestDirectionDto
{
    Inbound,
    Outbound
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizationRequestStatusDto
{
    Pending,
    Processing,
    Completed,
    Failed
}