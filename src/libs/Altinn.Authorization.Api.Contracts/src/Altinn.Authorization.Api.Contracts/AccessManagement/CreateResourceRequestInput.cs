namespace Altinn.Authorization.Api.Contracts.AccessManagement;

/// <summary>
/// Input for creating a new request for a resource
/// </summary>
public class CreateResourceRequestInput : CreateRequestInput
{
    /// <summary>
    /// Reference to the resource
    /// </summary>
    public ResourceReferenceDto Resource { get; set; }
}
