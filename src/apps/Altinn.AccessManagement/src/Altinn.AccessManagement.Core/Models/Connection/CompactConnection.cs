using Altinn.Authorization.Shared;
namespace Altinn.AccessManagement.Core.Models.Connection;

/// <summary>
/// Compact connection representation for core business logic
/// </summary>
public class CompactConnection
{
    /// <summary>
    /// Connected party information
    /// </summary>
    public Party Party { get; set; } = new();

    /// <summary>
    /// Roles associated with this connection
    /// </summary>
    public List<Role> Roles { get; set; } = new();

    /// <summary>
    /// Sub-connections under this connection
    /// </summary>
    public List<CompactConnection> Connections { get; set; } = new();
}

/// <summary>
/// Party information for core business logic
/// </summary>
public class Party
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
/// Role information for core business logic
/// </summary>
public class Role
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