using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Attribute assignment DTO for obligations
/// </summary>
public class ObligationAttributeAssignmentDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// XACML obligation DTO
/// </summary>
public class ObligationDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    public List<ObligationAttributeAssignmentDto> AttributeAssignments { get; set; } = [];
}

/// <summary>
/// Attribute assignment DTO for advice
/// </summary>
public class AdviceAttributeAssignmentDto
{
    [Required]
    public string AttributeId { get; set; } = string.Empty;
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public string? DataType { get; set; }
}

/// <summary>
/// XACML advice DTO
/// </summary>
public class AdviceDto
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    public List<AdviceAttributeAssignmentDto> AttributeAssignments { get; set; } = [];
}

/// <summary>
/// Status information DTO
/// </summary>
public class StatusDto
{
    [Required]
    public string Code { get; set; } = string.Empty;
    
    public string? Message { get; set; }
    public string? Detail { get; set; }
}

/// <summary>
/// Authorization response DTO for XACML decisions
/// </summary>
public class AuthorizationResponseDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DecisionDto Decision { get; set; }
    
    public List<ObligationDto> Obligations { get; set; } = [];
    public List<AdviceDto> Advice { get; set; } = [];
    public List<StatusDto> Status { get; set; } = [];
}