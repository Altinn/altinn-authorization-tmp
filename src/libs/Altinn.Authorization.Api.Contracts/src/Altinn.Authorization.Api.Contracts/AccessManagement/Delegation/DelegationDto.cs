using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

/// <summary>
/// Delegation right DTO
/// </summary>
public class DelegationRightDto
{
    [Required]
    public string Action { get; set; } = string.Empty;
    
    [Required]
    public string Resource { get; set; } = string.Empty;
    
    public List<AttributeMatchDto>? AttributeMatches { get; set; }
}

/// <summary>
/// Create delegation request DTO
/// </summary>
public class CreateDelegationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int OfferedByPartyId { get; set; }
    
    [Required]
    [Range(1, int.MaxValue)]
    public int CoveredByPartyId { get; set; }
    
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    [Range(1, int.MaxValue)]
    public int PerformedByUserId { get; set; }
    
    [Required]
    public List<DelegationRightDto> Rights { get; set; } = [];
}

/// <summary>
/// Update delegation request DTO
/// </summary>
public class UpdateDelegationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PerformedByUserId { get; set; }
    
    [Required]
    public List<DelegationRightDto> Rights { get; set; } = [];
}

/// <summary>
/// Delegation response DTO
/// </summary>
public class DelegationDto
{
    public Guid Id { get; set; }
    public int OfferedByPartyId { get; set; }
    public string OfferedByName { get; set; } = string.Empty;
    public string? OfferedByOrganizationNumber { get; set; }
    public int CoveredByPartyId { get; set; }
    public string CoveredByName { get; set; } = string.Empty;
    public string? CoveredByOrganizationNumber { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    public int PerformedByUserId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DelegationStatusDto Status { get; set; } = DelegationStatusDto.Active;
    
    public List<DelegationRightDto> Rights { get; set; } = [];
    public List<ResourceReferenceDto> ResourceReferences { get; set; } = [];
    public CompetentAuthorityDto? CompetentAuthority { get; set; }
}

/// <summary>
/// Delegation lookup request DTO
/// </summary>
public class DelegationLookupDto
{
    public int? OfferedByPartyId { get; set; }
    public int? CoveredByPartyId { get; set; }
    public string? ResourceId { get; set; }
    public string? Action { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DelegationStatusDto? Status { get; set; }
    
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool IncludeExpired { get; set; } = false;
}

/// <summary>
/// Revoke delegation request DTO
/// </summary>
public class RevokeDelegationDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PerformedByUserId { get; set; }
    
    public string? Reason { get; set; }
}