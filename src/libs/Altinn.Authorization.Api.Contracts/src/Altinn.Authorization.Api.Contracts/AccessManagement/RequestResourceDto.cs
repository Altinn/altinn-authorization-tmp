namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Response dto for a resource request
/// </summary>
public class RequestResourceDto : RequestDto
{
    /// <summary>
    /// Resource that is requested
    /// </summary>
    public ResourceReferenceDto Resource { get; set; }
}
