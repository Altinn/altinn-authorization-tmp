﻿namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Provider
/// </summary>
public class Provider
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
    /// Refrence Identifier (e.g. OrgNo)
    /// </summary>
    public string RefId { get; set; }

    /// <summary>
    /// Logo url
    /// </summary>
    public string LogoUrl { get; set; }

    /// <summary>
    /// Provider code (e.g ttd, brg, skd)
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The type of provider
    /// </summary>
    public Guid TypeId { get; set; }
}

/// <summary>
/// Extended Provider
/// </summary>
public class ExtProvider : Provider
{
    /// <summary>
    /// The type of provider
    /// </summary>
    public ProviderType Type { get; set; }
}
