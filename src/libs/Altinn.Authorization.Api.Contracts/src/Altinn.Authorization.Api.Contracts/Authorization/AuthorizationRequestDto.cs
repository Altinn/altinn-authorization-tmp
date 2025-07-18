using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Subject attribute DTO
/// </summary>
public class SubjectAttributeDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// Subject DTO (who is making the request)
/// </summary>
public class SubjectDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public List<SubjectAttributeDto> Attributes { get; set; } = [];
}

/// <summary>
/// Action attribute DTO
/// </summary>
public class ActionAttributeDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// Action DTO (what is being requested)
/// </summary>
public class ActionDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public List<ActionAttributeDto> Attributes { get; set; } = [];
}

/// <summary>
/// Resource attribute DTO
/// </summary>
public class ResourceAttributeDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// Resource DTO (what is being accessed)
/// </summary>
public class ResourceDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public List<ResourceAttributeDto> Attributes { get; set; } = [];
}

/// <summary>
/// Environment context DTO
/// </summary>
public class EnvironmentDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// Authorization request DTO for XACML decisions
/// </summary>
public class AuthorizationRequestDto
{
    [Required]
    public List<SubjectDto> Subjects { get; set; } = [];
    
    [Required]
    public List<ActionDto> Actions { get; set; } = [];
    
    [Required]
    public List<ResourceDto> Resources { get; set; } = [];
    
    public List<EnvironmentDto>? Environment { get; set; }
}