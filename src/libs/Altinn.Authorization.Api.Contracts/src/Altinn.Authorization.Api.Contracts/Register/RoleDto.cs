using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Register;

/// <summary>
/// Role right definition DTO
/// </summary>
public class RoleRightDto
{
    [Required]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    public string Resource { get; set; } = string.Empty;
    
    public List<AttributeMatchDto>? Conditions { get; set; }
    public bool IsMandatory { get; set; } = false;
}

/// <summary>
/// Role definition DTO
/// </summary>
public class RoleDto
{
    [Required]
    public string RoleCode { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoleTypeDto RoleType { get; set; }
    
    public bool IsDelegable { get; set; } = true;
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<RoleRightDto> Rights { get; set; } = [];
    public List<string> RequiredRoles { get; set; } = [];
}

/// <summary>
/// Role assignment DTO
/// </summary>
public class RoleAssignmentDto
{
    public Guid Id { get; set; }
    public int PartyId { get; set; }
    public int UserId { get; set; }
    
    [Required]
    public string RoleCode { get; set; } = string.Empty;
    
    public DateTime AssignedDate { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int AssignedByUserId { get; set; }
    public bool IsActive { get; set; } = true;
    
    public RegisterPartyDto? Party { get; set; }
    public UserProfileDto? User { get; set; }
    public UserProfileDto? AssignedBy { get; set; }
    public RoleDto? Role { get; set; }
}

/// <summary>
/// Role lookup request DTO
/// </summary>
public class RoleLookupDto
{
    public string? RoleCode { get; set; }
    public string? Name { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoleTypeDto? RoleType { get; set; }
    
    public bool? IsDelegable { get; set; }
    public bool IncludeInactive { get; set; } = false;
    public bool IncludeRights { get; set; } = false;
    public DateTime? ValidAt { get; set; }
}