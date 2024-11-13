﻿namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Area to organize accesspackages stuff
/// </summary>
public class Area
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// IconName
    /// </summary>
    public string IconName { get; set; }

    /// <summary>
    /// GroupId
    /// </summary>
    public Guid GroupId { get; set; }
}

/// <summary>
/// Extended Area
/// </summary>
public class ExtArea : Area
{
    /// <summary>
    /// Group
    /// </summary>
    public AreaGroup Group { get; set; }
}