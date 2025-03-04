using Altinn.AccessMgmt.Core.Models;

namespace Altinn.AccessMgmt.Persistence.Services.Models;

/// <summary>
/// Represents a package with related metadata and associated resources.
/// </summary>
public class PackageDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the package.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the package.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique resource name (URN) for the package.
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Gets or sets the description of the package.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the area associated with the package.
    /// </summary>
    public ExtArea Area { get; set; }

    /// <summary>
    /// Gets or sets the collection of resources linked to the package.
    /// </summary>
    public IEnumerable<Resource> Resources { get; set; }
}

/// <summary>
/// Represents a group of areas, categorized under a specific entity type.
/// </summary>
public class AreaGroupDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the area group.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the area group.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique resource name (URN) for the area group.
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Gets or sets the description of the area group.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the type of entity this area group represents.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the list of areas that belong to this group.
    /// </summary>
    public List<AreaDto> Areas { get; set; }
}

/// <summary>
/// Represents an area with relevant metadata and associated packages.
/// </summary>
public class AreaDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the area.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the area.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique resource name (URN) for the area.
    /// </summary>
    public string Urn { get; set; }

    /// <summary>
    /// Gets or sets the description of the area.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the icon representing the area.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets the list of packages available in this area.
    /// </summary>
    public List<PackageDto> Packages { get; set; }
}
