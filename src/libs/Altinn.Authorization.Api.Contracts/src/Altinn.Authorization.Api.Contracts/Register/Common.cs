using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Register;

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

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContactTypeDto
{
    Email,
    Phone,
    Mobile,
    Fax,
    Website
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AddressTypeDto
{
    Business,
    Postal,
    Visiting,
    Home
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RelationshipTypeDto
{
    Parent,
    Child,
    Subsidiary,
    Branch,
    Representative,
    Owner
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserPartyRelationTypeDto
{
    Employee,
    Representative,
    Owner,
    Authorized,
    Contact
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RoleTypeDto
{
    System,
    Business,
    Delegation,
    AccessList
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartyTypeDto
{
    Person,
    Organization,
    SubUnit
}