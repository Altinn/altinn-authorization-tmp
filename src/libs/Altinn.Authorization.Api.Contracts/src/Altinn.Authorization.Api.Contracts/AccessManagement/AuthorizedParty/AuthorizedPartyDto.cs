using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Altinn.Authorization.Api.Contracts.AccessManagement.Common;
using Altinn.Authorization.Api.Contracts.AccessManagement.Delegation;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.AuthorizedParty;

/// <summary>
/// Authorized party right DTO
/// </summary>
public class AuthorizedPartyRightDto
{
    public string Action { get; set; } = string.Empty;
    public string Resource { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RightSourceTypeDto Source { get; set; }
    
    public List<AttributeMatchDto> AttributeMatches { get; set; } = [];
}

/// <summary>
/// Authorized party resource DTO
/// </summary>
public class AuthorizedPartyResourceDto
{
    public string ResourceId { get; set; } = string.Empty;
    public string? ResourceType { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public List<string> Actions { get; set; } = [];
}

/// <summary>
/// Authorized party DTO
/// </summary>
public class AuthorizedPartyDto
{
    public int PartyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? OrganizationNumber { get; set; }
    public string? PersonIdentifier { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeDto PartyType { get; set; }
    
    public List<AuthorizedPartyRightDto> Rights { get; set; } = [];
    public List<AuthorizedPartyResourceDto> Resources { get; set; } = [];
    public List<DelegationDto> Delegations { get; set; } = [];
}

/// <summary>
/// Authorized party lookup request DTO
/// </summary>
public class AuthorizedPartyLookupDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int SubjectUserId { get; set; }
    
    public string? ResourceId { get; set; }
    public string? Action { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeDto? PartyType { get; set; }
    
    public bool IncludeAltinn2 { get; set; } = false;
    public bool IncludeExpired { get; set; } = false;
}