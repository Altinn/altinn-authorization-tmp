namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Delegation check model for a resource
/// </summary>
public class ResourceCheckDto
{
    /// <summary>
    /// Resource the delegation check is regarding
    /// </summary>
    public required ResourceDto Resource { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public required IEnumerable<ActionDto> Actions { get; set; }
}
