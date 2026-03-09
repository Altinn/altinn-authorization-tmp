namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Input for creating a new request for an access package
/// </summary>
public class CreatePackageRequestInput : CreateRequestInput
{
    /// <summary>
    /// Reference to the access package
    /// </summary>
    public PackageReferenceDto Package { get; set; }
}
