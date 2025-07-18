using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement.Common;

/// <summary>
/// Person DTO for external contracts
/// </summary>
public class PersonDto
{
    public string SSN { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string MiddleName { get; set; }
    public string LastName { get; set; }
    public string TelephoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public string EMailAddress { get; set; }
    public string AddressPostalCode { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreetName { get; set; }
    public string AddressHouseNumber { get; set; }
    public string AddressHouseLetter { get; set; }
    public string AddressPostalCodeName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime DateOfDeath { get; set; }
}

/// <summary>
/// Organization DTO for external contracts
/// </summary>
public class OrganizationDto
{
    public string OrgNumber { get; set; }
    public string Name { get; set; }
    public string UnitType { get; set; }
    public string TelephoneNumber { get; set; }
    public string MobileNumber { get; set; }
    public string FaxNumber { get; set; }
    public string EMailAddress { get; set; }
    public string InternetAddress { get; set; }
    public string AddressPostalCode { get; set; }
    public string AddressCity { get; set; }
    public string AddressStreetName { get; set; }
    public string AddressHouseNumber { get; set; }
    public string AddressHouseLetter { get; set; }
    public string AddressPostalCodeName { get; set; }
    public string BusinessAddress { get; set; }
    public string BusinessPostalCode { get; set; }
    public string BusinessPostalCity { get; set; }
}

/// <summary>
/// Party DTO for external contracts
/// </summary>
public class PartyDto
{
    public int PartyId { get; set; }
    public PartyTypeDto PartyTypeName { get; set; }
    public string OrgNumber { get; set; }
    public string SSN { get; set; }
    public string UnitType { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; }
    public bool OnlyHierarchyElementWithNoAccess { get; set; }
    public PersonDto Person { get; set; }
    public OrganizationDto Organization { get; set; }
    public List<PartyDto> ChildParties { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartyTypeDto
{
    Person,
    Organization,
    SubUnit
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizedPartyTypeExternal
{
    Person,
    Organization,
    SubUnit
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UuidTypeExternal
{
    Person,
    Organization,
    SystemUser
}