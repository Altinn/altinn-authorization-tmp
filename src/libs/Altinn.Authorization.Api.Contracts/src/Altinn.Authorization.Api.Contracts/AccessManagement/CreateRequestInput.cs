namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Base input dto for creating a new request
/// </summary>
public class CreateRequestInput
{
    /// <summary>
    /// Request connection
    /// </summary>
    public ConnectionRequestInputDto Connection { get; set; }

    /// <summary>
    /// Reference to the resource
    /// </summary>
    public ResourceReferenceDto Resource { get; set; }

    /// <summary>
    /// Reference to the access package
    /// </summary>
    public PackageReferenceDto Package { get; set; }
}
