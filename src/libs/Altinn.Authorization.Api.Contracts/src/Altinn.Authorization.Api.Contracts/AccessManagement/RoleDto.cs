using System.Text.Json.Serialization;

namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Represents a group of areas, categorized under a specific entity type.
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// e.g Dagligleder
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Code
    /// e.g daglig-leder
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Description
    /// e.g The main operator of the organization
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Defines the role as a KeyRole
    /// </summary>
    public bool IsKeyRole { get; set; }

    /// <summary>
    /// Urn
    /// e.g altinn:external-role:ccr:daglig-leder
    /// altinn:role:tilgangsstyrer
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Legacy role code
    /// </summary>
    public string LegacyRoleCode { get; set; }

    /// <summary>
    /// Legacy Urn
    /// </summary>
    public string LegacyUrn { get; set; }

    /// <summary>
    /// Indicates if the package can be used as subject for authorization in resource policy
    /// </summary>
    public bool IsResourcePolicyAvailable { get; set; }

    /// <summary>
    /// Provider
    /// </summary>
    public ProviderDto Provider { get; set; }

    /// <summary>
    /// There exist a role having role provider Altinn2 and a permission that is direct.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsRevocable { get; set; } = null;
}
