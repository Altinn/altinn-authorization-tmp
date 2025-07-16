using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Policy information DTO
/// </summary>
public class PolicyDto
{
    [Required]
    public string PolicyId { get; set; } = string.Empty;
    
    [Required]
    public string Version { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty; // XACML policy as XML string
    
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PolicyStatusDto Status { get; set; } = PolicyStatusDto.Active;
    
    public List<string> Tags { get; set; } = [];
}

/// <summary>
/// Policy search request DTO
/// </summary>
public class PolicySearchDto
{
    public string? PolicyId { get; set; }
    public string? ResourceId { get; set; }
    public string? SubjectId { get; set; }
    public string? Action { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PolicyStatusDto? Status { get; set; }
    
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Policy validation error DTO
/// </summary>
public class ValidationErrorDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Location { get; set; }
    public int? Line { get; set; }
    public int? Column { get; set; }
}

/// <summary>
/// Policy validation warning DTO
/// </summary>
public class ValidationWarningDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
    
    [Required]
    public string Message { get; set; } = string.Empty;
    
    public string? Location { get; set; }
}

/// <summary>
/// Policy validation result DTO
/// </summary>
public class PolicyValidationDto
{
    public bool IsValid { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = [];
    public List<ValidationWarningDto> Warnings { get; set; } = [];
}