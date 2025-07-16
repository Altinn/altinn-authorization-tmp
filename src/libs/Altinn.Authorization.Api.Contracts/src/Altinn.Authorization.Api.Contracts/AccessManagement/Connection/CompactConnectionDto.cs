namespace Altinn.Authorization.Api.Contracts.AccessManagement.Connection;

/// <summary>
/// Compact connection representation for API responses
/// </summary>
public class CompactConnectionDto
{
    /// <summary>
    /// Connected party information
    /// </summary>
    public PartyDto Party { get; set; } = new();

    /// <summary>
    /// Roles associated with this connection
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();

    /// <summary>
    /// Sub-connections under this connection
    /// </summary>
    public List<CompactConnectionDto> Connections { get; set; } = new();
}

/// <summary>
/// Party information for connections
/// </summary>
public class PartyDto
{
    /// <summary>
    /// Party identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Party name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Party type (Person, Organization, etc.)
    /// </summary>
    public string PartyType { get; set; } = string.Empty;

    /// <summary>
    /// Organization number (if applicable)
    /// </summary>
    public string? OrganizationNumber { get; set; }

    /// <summary>
    /// Person identifier (if applicable)
    /// </summary>
    public string? PersonId { get; set; }
}

/// <summary>
/// Role information for connections
/// </summary>
public class RoleDto
{
    /// <summary>
    /// Role identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Role name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}