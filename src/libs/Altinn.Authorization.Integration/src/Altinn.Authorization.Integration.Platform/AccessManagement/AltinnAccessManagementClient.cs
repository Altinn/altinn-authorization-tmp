using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Platform.AccessManagement;

/// <summary>
/// Client for interacting with the Altinn Register service.
/// </summary>
/// <param name="HttpClientFactory">Factory for creating HTTP clients.</param>
/// <param name="Options">Configuration options for the Altinn Register service.</param>
public partial class AltinnAccessManagementClient(
    IHttpClientFactory HttpClientFactory,
    IOptions<AltinnAccessManagementClient> Options
) : IAltinnAccessManagement
{
}

/// <summary>
/// Interface for interacting with the Altinn Register service.
/// </summary>
public interface IAltinnAccessManagement
{
}
