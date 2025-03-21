using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.Integration.Platform.AltinnRole;

/// <summary>
/// Client for interacting with endpoint that stream roles (ER-roles).
/// </summary>
public partial class AltinnRoleClient
{
    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<PlatformResponse<PageStream<RoleDelegationModel>>>> StreamRoles(string subscriptionId, string nextPage = null, CancellationToken cancellationToken = default)
    {
        List<Action<HttpRequestMessage>> request = [
            RequestComposer.WithHttpVerb(HttpMethod.Get),
            RequestComposer.WithSetUri(Options.Value.Endpoint, $"/sblbridge/roledelegationevent/api/getevents?subscriptionId={subscriptionId}"),
            RequestComposer.WithSetUri(nextPage)
        ];

        var response = await HttpClient.SendAsync(RequestComposer.New([.. request]), cancellationToken);

        return new PaginatorStream<RoleDelegationModel>(HttpClient, response, request);
    }
}

/// <summary>
/// Represents a party with various personal and organizational details.
/// This model is used to deserialize JSON responses containing party information.
/// </summary>
[ExcludeFromCodeCoverage]
public class RoleDelegationModel
{
    /// <summary>
    /// Gets or sets the unique identifier for the Altinn role delegation event.
    /// </summary>
    [JsonPropertyName("altinnRoleDelegationEventId")]
    public long AltinnRoleDelegationEventId { get; set; }

    /// <summary>
    /// Gets or sets the action taken in the delegation (Delegation/Revoke).
    /// </summary>
    [JsonPropertyName("delegationAction")]
    public DelegationAction DelegationAction { get; set; }

    /// <summary>
    /// Gets or sets the role type code.
    /// </summary>
    [JsonPropertyName("roleTypeCode")]
    public string RoleTypeCode { get; set; }

    /// <summary>
    /// Gets or sets the ID of the party from which the role is delegated.
    /// </summary>
    [JsonPropertyName("fromPartyId")]
    public int FromPartyId { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the party from which the role is delegated.
    /// </summary>
    [JsonPropertyName("fromPartyUuid")]
    public Guid FromPartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user to whom the role is delegated.
    /// </summary>
    [JsonPropertyName("delegationAction")]
    public int? ToUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the party to which the role is delegated.
    /// </summary>
    [JsonPropertyName("toPartyId")]
    public int? ToPartyId { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the user party to which the role is delegated.
    /// </summary>
    [JsonPropertyName("delegationAction")]
    public Guid ToUserPartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the delegation change occurred.
    /// </summary>
    [JsonPropertyName("delegationChangeDateTime")]
    public DateTimeOffset? DelegationChangeDateTime { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who performed the delegation.
    /// </summary>
    [JsonPropertyName("performedByUserId")]
    public int? PerformedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the party who performed the delegation.
    /// </summary>
    [JsonPropertyName("performedByPartyId")]
    public int? PerformedByPartyId { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the user who performed the delegation.
    /// </summary>
    [JsonPropertyName("performedByUserUuid")]
    public Guid? PerformedByUserUuid { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the party who performed the delegation.
    /// </summary>
    [JsonPropertyName("performedByPartyUuid")]
    public Guid? PerformedByPartyUuid { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role.
    /// </summary>
    [JsonPropertyName("roleId")]
    public int? RoleId { get; set; }

    /// <summary>
    /// Gets or sets the ID of the role delegation.
    /// </summary>
    [JsonPropertyName("roleDelegationId")]
    public int? RoleDelegationId { get; set; }
}

/// <summary>
/// Enumeration for which delegation action was performed
/// </summary>
public enum DelegationAction : int
{
    /// <summary>
    /// Value indicating no value has been set
    /// </summary>
    [EnumMember]
    NotSet = 0,

    /// <summary>
    /// Value indicating that this was a delegation
    /// </summary>
    [EnumMember]
    Delegate = 1,

    /// <summary>
    /// Value indicating that this was a revoke
    /// </summary>
    [EnumMember]
    Revoke = 2,

    /// <summary>
    /// Value indicating that this was a delete based on a soft delete of a DelegationScheme
    /// </summary>
    [EnumMember]
    DelegationSchemeCascadingDelete = 3
}
