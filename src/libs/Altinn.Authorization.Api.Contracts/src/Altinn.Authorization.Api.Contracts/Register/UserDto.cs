using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Register;

/// <summary>
/// User-party relationship DTO
/// </summary>
public class UserPartyRelationDto
{
    public int PartyId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserPartyRelationTypeDto RelationType { get; set; }
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    
    public RegisterPartyDto? Party { get; set; }
}

/// <summary>
/// User preferences DTO
/// </summary>
public class UserPreferencesDto
{
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? NumberFormat { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public Dictionary<string, object>? CustomSettings { get; set; }
}

/// <summary>
/// User profile DTO
/// </summary>
public class UserProfileDto
{
    public int UserId { get; set; }
    
    [Required]
    public string UserName { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? PreferredLanguage { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastLogin { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<UserPartyRelationDto> PartyRelations { get; set; } = [];
    public UserPreferencesDto? Preferences { get; set; }
}

/// <summary>
/// User lookup request DTO
/// </summary>
public class UserLookupDto
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PersonIdentifier { get; set; }
    public bool IncludePartyRelations { get; set; } = false;
    public bool IncludePreferences { get; set; } = false;
    public bool IncludeInactive { get; set; } = false;
}