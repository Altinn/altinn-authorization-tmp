namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Response dto for an access package request
/// </summary>
public class RequestPackageDto : RequestDto
{
    /// <summary>
    /// Access package that is requested
    /// </summary>
    public PackageReferenceDto Package { get; set; }
}
