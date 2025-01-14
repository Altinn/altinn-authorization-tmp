namespace Altinn.AccessMgmt.Models;

/// <summary>
/// ResourceGroup
/// </summary>
public class ResourceGroup
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
    /// Name
    /// </summary>
    public string Name { get; set; }
}

/// <summary>
/// Extended ResourceGroup
/// </summary>
public class ExtResourceGroup : ResourceGroup
{
    /// <summary>
    /// Provider
    /// </summary>
    public Provider Provider { get; set; }
}
