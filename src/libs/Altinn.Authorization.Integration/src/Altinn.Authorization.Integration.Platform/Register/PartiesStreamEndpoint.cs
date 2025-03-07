using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Platform.Register;

/// <summary>
/// Client for interacting with endpoint that stream parties (organization, users and systemusers).
/// </summary>
public partial class RegisterClient
{
    /// <summary>
    /// List of available fields that can be retrieved from the register.
    /// </summary>
    private readonly IEnumerable<string> _availableFields = [
        "party",
        "org",
        "person",
        "identifiers",
        "person.name",
        "org.fax",
        "org.internet",
        "org.mobile",
        "person.mailing-address",
        "org.type",
        "org.status",
        "org.mailing-address",
        "org.business-address",
        "person.date-of-death",
        "person.date-of-birth",
        "org.email",
        "org.telephone",
        "uuid",
        "person.short-name",
        "person.last-name",
        "person.middle-name",
        "person.first-name",
        "version",
        "deleted",
        "modified",
        "created",
        "org-id",
        "person-id",
        "display-name",
        "type",
        "id",
        "person.address"
    ];

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<PlatformResponse<PageStream<PartyModel>>>> StreamParties(IEnumerable<string> fields, string nextPage = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Get),
            RequestComposer.WithSetUri(Options.Value.Endpoint, "/register/api/v2/internal/parties/stream"),
            RequestComposer.WithSetUri(nextPage),
            RequestComposer.WithAppendQueryParam("fields", fields),
            RequestComposer.WithPlatformAccessToken(AccessTokenGenerator, "access-management")
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return new PaginatorStream<PartyModel>(HttpClient, response, request);
    }
}

/// <summary>
/// Represents a party with various personal and organizational details.
/// This model is used to deserialize JSON responses containing party information.
/// </summary>
[ExcludeFromCodeCoverage]
public class PartyModel
{
    /// <summary>
    /// Gets or sets the type of the party.
    /// </summary>
    [JsonPropertyName("partyType")]
    public string PartyType { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the party.
    /// </summary>
    [JsonPropertyName("partyUuid")]
    public string PartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the party's ID.
    /// </summary>
    [JsonPropertyName("partyId")]
    public int PartyId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the party.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the identifier for an individual person.
    /// </summary>
    [JsonPropertyName("personIdentifier")]
    public string PersonIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the identifier for an organization.
    /// </summary>
    [JsonPropertyName("organizationIdentifier")]
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the creation date of the party record.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modified date of the party record.
    /// </summary>
    [JsonPropertyName("modifiedAt")]
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the party record is deleted.
    /// </summary>
    [JsonPropertyName("isDeleted")]
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the version ID of the party record.
    /// </summary>
    [JsonPropertyName("versionId")]
    public int VersionId { get; set; }

    /// <summary>
    /// Gets or sets the first name of the party (if applicable).
    /// </summary>
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; }

    /// <summary>
    /// Gets or sets the middle name of the party (if applicable).
    /// </summary>
    [JsonPropertyName("middleName")]
    public string MiddleName { get; set; }

    /// <summary>
    /// Gets or sets the last name of the party (if applicable).
    /// </summary>
    [JsonPropertyName("lastName")]
    public string LastName { get; set; }

    /// <summary>
    /// Gets or sets the short name of the party.
    /// </summary>
    [JsonPropertyName("shortName")]
    public string ShortName { get; set; }

    /// <summary>
    /// Gets or sets the physical address of the party.
    /// </summary>
    [JsonPropertyName("address")]
    public PartyAddress Address { get; set; }

    /// <summary>
    /// Gets or sets the mailing address of the party.
    /// </summary>
    [JsonPropertyName("mailingAddress")]
    public PartyMailingAddress MailingAddress { get; set; }

    /// <summary>
    /// Gets or sets the date of birth of the party (if applicable).
    /// </summary>
    [JsonPropertyName("dateOfBirth")]
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the date of death of the party (if applicable).
    /// </summary>
    [JsonPropertyName("dateOfDeath")]
    public DateTime? DateOfDeath { get; set; }

    /// <summary>
    /// Represents the physical address details of a party.
    /// </summary>
    public class PartyAddress
    {
        /// <summary>
        /// Gets or sets the municipal number of the address.
        /// </summary>
        [JsonPropertyName("municipalNumber")]
        public string MunicipalNumber { get; set; }

        /// <summary>
        /// Gets or sets the municipal name of the address.
        /// </summary>
        [JsonPropertyName("municipalName")]
        public string MunicipalName { get; set; }

        /// <summary>
        /// Gets or sets the street name of the address.
        /// </summary>
        [JsonPropertyName("streetName")]
        public string StreetName { get; set; }

        /// <summary>
        /// Gets or sets the house number of the address.
        /// </summary>
        [JsonPropertyName("houseNumber")]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Gets or sets the house letter of the address (if applicable).
        /// </summary>
        [JsonPropertyName("houseLetter")]
        public string HouseLetter { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the address.
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the city of the address.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }
    }

    /// <summary>
    /// Represents the mailing address details of a party.
    /// </summary>
    public class PartyMailingAddress
    {
        /// <summary>
        /// Gets or sets the mailing address.
        /// </summary>
        [JsonPropertyName("address")]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the postal code of the mailing address.
        /// </summary>
        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Gets or sets the city of the mailing address.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }
    }
}
