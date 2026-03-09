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
}
