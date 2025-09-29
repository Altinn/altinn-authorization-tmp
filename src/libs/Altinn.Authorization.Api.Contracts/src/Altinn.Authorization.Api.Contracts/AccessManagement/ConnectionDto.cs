﻿namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Connection from one party to another
/// </summary>
public class ConnectionDto
{
    /// <summary>
    /// Party
    /// </summary>
    public CompactEntityDto Party { get; set; } = new();

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
    public List<CompactRoleDto> Roles { get; set; } = new();

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<ConnectionDto> Connections { get; set; } = new();
}

/// <summary>
/// Connection from one party to another
/// </summary>
public class BasicConnectionDto
{
    /// <summary>
    /// Party
    /// </summary>
    public Entity From { get; set; } = new();

    /// <summary>
    /// Party
    /// </summary>
    public Entity To { get; set; } = new();

    /// <summary>
    /// Party
    /// </summary>
    public Entity Via { get; set; } = new();

    /// <summary>
    /// Party
    /// </summary>
    public RoleDto Role { get; set; } = new();

    /// <summary>
    /// Party
    /// </summary>
    public RoleDto ViaRole { get; set; } = new();
}

/// <summary>
/// Connection from one party to another
/// </summary>
public class ConnectionPackageDto
{
    /// <summary>
    /// Party
    /// </summary>
    public CompactEntityDto Party { get; set; } = new();

    /// <summary>
    /// Roles the party has for given filter
    /// </summary>
    public List<CompactRoleDto> Roles { get; set; } = new();

    /// <summary>
    /// Connections the party has
    /// </summary>
    public List<ConnectionDto> Connections { get; set; } = new();

    /// <summary>
    /// Packages the party has
    /// </summary>
    public List<CompactPackageDto> Packages { get; set; } = new();
}
