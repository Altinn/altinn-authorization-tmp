using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.AccessPackages.Models;

/// <summary>
/// TagGroup
/// </summary>
public class TagGroup
{
    /// <summary>
    /// Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// TagGroup
/// </summary>
[Experimental("ExpandList")]
public class ExtTagGroup : TagGroup
{
    /// <summary>
    /// Tags
    /// </summary>
    public IEnumerable<Tag> Tags { get; set; }
}