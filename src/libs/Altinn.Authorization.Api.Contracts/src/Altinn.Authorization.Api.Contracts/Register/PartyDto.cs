using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.Register;

public class PartyDto
{
    public class Extended
    {
        [JsonPropertyName("partyType")]
        public string PartyType { get; set; }

        [JsonPropertyName("partyUuid")]
        public string PartyUuid { get; set; }

        [JsonPropertyName("partyId")]
        public int PartyId { get; set; }

        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        [JsonPropertyName("personIdentifier")]
        public string PersonIdentifier { get; set; }

        [JsonPropertyName("organizationIdentifier")]
        public string OrganizationIdentifier { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [JsonPropertyName("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonPropertyName("versionId")]
        public int VersionId { get; set; }

        [JsonPropertyName("unitStatus")]
        public string UnitStatus { get; set; }

        [JsonPropertyName("unitType")]
        public string UnitType { get; set; }

        [JsonPropertyName("telephoneNumber")]
        public string TelephoneNumber { get; set; }

        [JsonPropertyName("mobileNumber")]
        public string MobileNumber { get; set; }

        [JsonPropertyName("faxNumber")]
        public string FaxNumber { get; set; }

        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("internetAddress")]
        public string InternetAddress { get; set; }

        [JsonPropertyName("mailingAddress")]
        public PartyMailingAddress MailingAddress { get; set; }

        [JsonPropertyName("businessAddress")]
        public PartyBusinessAddress BusinessAddress { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("middleName")]
        public object MiddleName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("shortName")]
        public string ShortName { get; set; }

        [JsonPropertyName("address")]
        public PartyAddress Address { get; set; }

        [JsonPropertyName("dateOfBirth")]
        public string DateOfBirth { get; set; }

        [JsonPropertyName("dateOfDeath")]
        public string DateOfDeath { get; set; }

        [JsonPropertyName("user")]
        public UserProfile User { get; set; }
    }

    public class UserProfile
    {
        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userIds")]
        public IEnumerable<int> UserIds { get; set; }
    }

    public class PartyAddress
    {
        [JsonPropertyName("municipalNumber")]
        public string MunicipalNumber { get; set; }

        [JsonPropertyName("municipalName")]
        public string MunicipalName { get; set; }

        [JsonPropertyName("streetName")]
        public string StreetName { get; set; }

        [JsonPropertyName("houseNumber")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("houseLetter")]
        public object HouseLetter { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    public class PartyBusinessAddress
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    public class PartyMailingAddress
    {
        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }
    }
}
