﻿namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Resource
/// </summary>
public class Resource
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ProviderId
    /// </summary>
    public Guid ProviderId { get; set; }

    /// <summary>
    /// GroupId
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// TypeId
    /// </summary>
    public Guid TypeId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Extended Resource
/// </summary>
public class ExtResource : Resource
{
    /// <summary>
    /// Provider
    /// </summary>
    public Provider Provider { get; set; }

    /// <summary>
    /// Group
    /// </summary>
    public ResourceGroup Group { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public ResourceType Type { get; set; }
}