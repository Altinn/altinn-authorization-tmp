using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Register.Models;

// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
// Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);
public class PartyModel
{
    [JsonPropertyName("partyUuid")]
    public string PartyUuid { get; set; }

    [JsonPropertyName("partyId")]
    public int PartyId { get; set; }

    [JsonPropertyName("partyType")]
    public string PartyType { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

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

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    [JsonPropertyName("middleName")]
    public string MiddleName { get; set; }

    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    [JsonPropertyName("address")]
    public PartyAddress Address { get; set; }

    [JsonPropertyName("mailingAddress")]
    public PartyMailingAddress MailingAddress { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public string DateOfBirth { get; set; }

    [JsonPropertyName("dateOfDeath")]
    public string DateOfDeath { get; set; }

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
        public string HouseLetter { get; set; }

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
