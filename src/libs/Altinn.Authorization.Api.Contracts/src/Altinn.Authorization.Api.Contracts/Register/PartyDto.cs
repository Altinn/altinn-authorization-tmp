using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Register;

/// <summary>
/// Party contact information DTO
/// </summary>
public class PartyContactDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ContactTypeDto ContactType { get; set; }
    
    [Required]
    public string Value { get; set; } = string.Empty;
    
    public bool IsPrimary { get; set; }
}

/// <summary>
/// Party address information DTO
/// </summary>
public class PartyAddressDto
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AddressTypeDto AddressType { get; set; }
    
    public string? StreetAddress { get; set; }
    public string? PostalCode { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
}

/// <summary>
/// Additional party details DTO
/// </summary>
public class PartyDetailsDto
{
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? Industry { get; set; }
    public int? EmployeeCount { get; set; }
    public DateTime? FoundedDate { get; set; }
    public Dictionary<string, string>? CustomAttributes { get; set; }
}

/// <summary>
/// Party information DTO for Register API
/// </summary>
public class RegisterPartyDto
{
    public int PartyId { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeDto PartyType { get; set; }
    
    public string? OrganizationNumber { get; set; }
    public string? PersonIdentifier { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Updated { get; set; }
    
    public List<PartyContactDto> Contacts { get; set; } = [];
    public List<PartyAddressDto> Addresses { get; set; } = [];
    public PartyDetailsDto? Details { get; set; }
}

/// <summary>
/// Party lookup request DTO
/// </summary>
public class PartyLookupDto
{
    public int? PartyId { get; set; }
    public string? OrganizationNumber { get; set; }
    public string? PersonIdentifier { get; set; }
    public string? Name { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PartyTypeDto? PartyType { get; set; }
    
    public bool IncludeDeleted { get; set; } = false;
    public bool IncludeContacts { get; set; } = false;
    public bool IncludeAddresses { get; set; } = false;
    public bool IncludeDetails { get; set; } = false;
}

/// <summary>
/// Party hierarchy request DTO
/// </summary>
public class PartyHierarchyDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int PartyId { get; set; }
    
    public bool IncludeChildren { get; set; } = true;
    public bool IncludeParents { get; set; } = false;
    public int MaxDepth { get; set; } = 10;
}

/// <summary>
/// Party relationship DTO
/// </summary>
public class PartyRelationshipDto
{
    public int FromPartyId { get; set; }
    public int ToPartyId { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RelationshipTypeDto RelationshipType { get; set; }
    
    public DateTime ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    
    public RegisterPartyDto? FromParty { get; set; }
    public RegisterPartyDto? ToParty { get; set; }
}