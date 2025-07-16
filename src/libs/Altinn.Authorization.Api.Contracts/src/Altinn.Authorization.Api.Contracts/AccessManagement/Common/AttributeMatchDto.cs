using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

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

/// <summary>
/// Attribute match for contracts (External)
/// </summary>
public class AttributeMatchExternal
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeMatchTypeDto Type { get; set; } = AttributeMatchTypeDto.Equals;
    
    public string? DataType { get; set; }
}

/// <summary>
/// Policy attribute match for contracts
/// </summary>
public class PolicyAttributeMatchExternal
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AttributeMatchTypeDto Type { get; set; } = AttributeMatchTypeDto.Equals;
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