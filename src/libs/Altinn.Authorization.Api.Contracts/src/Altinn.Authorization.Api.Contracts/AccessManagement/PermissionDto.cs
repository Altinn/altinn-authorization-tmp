﻿namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Permission
/// </summary>
public class PermissionDto
{
    /// <summary>
    /// From party
    /// </summary>
    public CompactEntityDto From { get; set; }

    /// <summary>
    /// To party
    /// </summary>
    public CompactEntityDto To { get; set; }

    /// <summary>
    /// Via party
    /// </summary>
    public CompactEntityDto Via { get; set; }

    /// <summary>
    /// Role
    /// </summary>
    public CompactRoleDto Role { get; set; }

    /// <summary>
    /// Via role
    /// </summary>
    public CompactRoleDto ViaRole { get; set; }
}
