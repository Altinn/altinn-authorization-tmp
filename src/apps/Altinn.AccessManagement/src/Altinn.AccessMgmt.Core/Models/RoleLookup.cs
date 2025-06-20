﻿using Altinn.AccessManagement.Core.Extensions;

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Lookup values for role
/// </summary>
public class RoleLookup
{
    private Guid _id;

    /// <summary>
    /// Initializes a new instance of the <see cref="RoleLookup"/> class.
    /// </summary>
    public RoleLookup()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id
    {
        get => _id;
        set
        {
            if (!value.IsVersion7Uuid())
            {
                throw new ArgumentException("Id must be a version 7 UUID", nameof(value));
            }

            _id = value;
        }
    }

    /// <summary>
    /// Role identifier
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Key (e.g Party,SSN,OrgNo)
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// Extended role lookup
/// </summary>
public class ExtRoleLookup : RoleLookup
{
    /// <summary>
    /// Role
    /// </summary>
    public Role Role { get; set; }
}

/// <summary>
/// Extended role lookup
/// </summary>
public class ExtendedRoleLookup : RoleLookup
{
    /// <summary>
    /// Role
    /// </summary>
    public ExtendedRole Role { get; set; }
}
