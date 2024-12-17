namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// Package
/// </summary>
public class Package
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
    /// EntityTypeId
    /// </summary>
    public Guid EntityTypeId { get; set; }

    /// <summary>
    /// AreaId
    /// </summary>
    public Guid AreaId { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// IsDelegable
    /// </summary>
    public bool IsDelegable { get; set; }

    /// <summary>
    /// Has resources
    /// </summary>
    public bool HasResources { get; set; }
}

/// <summary>
/// Extended Package
/// </summary>
public class ExtPackage : Package
{
    /// <summary>
    /// Provider
    /// </summary>
    public Provider Provider { get; set; }

    /// <summary>
    /// EntityType
    /// </summary>
    public EntityType EntityType { get; set; }

    /// <summary>
    /// Area
    /// </summary>
    public Area Area { get; set; }
}

public class Meta
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Ingress { get; set; }
    public string Body { get; set; }
}

public class MetaSection
{
    public Guid Id { get; set; }
    public Guid MetaId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
}

public class MetaLink
{
    public Guid Id { get; set; }
    public Guid MetaId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
}
