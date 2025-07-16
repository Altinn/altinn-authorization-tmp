using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.Api.Contracts.Authorization;

/// <summary>
/// Access list authorization request DTO
/// </summary>
public class AccessListAuthorizationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectPartyId { get; set; }
    
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    public string Action { get; set; } = string.Empty;
    
    public List<AttributeMatchDto>? ResourceAttributes { get; set; }
}

/// <summary>
/// Access list authorization response DTO
/// </summary>
public class AccessListAuthorizationResponseDto
{
    public bool IsAuthorized { get; set; }
    public List<string> AccessLists { get; set; } = [];
    public string? Reason { get; set; }
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}