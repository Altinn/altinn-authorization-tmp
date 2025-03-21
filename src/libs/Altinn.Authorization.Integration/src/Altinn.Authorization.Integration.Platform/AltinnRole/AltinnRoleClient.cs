using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.Integration.Platform.AltinnRole;

/// <summary>
/// Client for interacting with the Altinn Register service.
/// </summary>
/// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
/// <param name="options">Configuration options for the Altinn Register service.</param>
public partial class AltinnRoleClient(IHttpClientFactory httpClientFactory, IOptions<AltinnRoleOptions> options) : IAltinnRole
{
    /// <summary>
    /// The name of the HTTP client used to communicate with the Altinn Register service.
    /// </summary>
    internal const string HttpClientName = "Altinn Role";

    private HttpClient HttpClient => HttpClientFactory.CreateClient(HttpClientName);

    private IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;

    private IOptions<AltinnRoleOptions> Options { get; } = options;
}

/// <summary>
/// Interface for interacting with the Altinn Register service.
/// </summary>
public interface IAltinnRole
{
    /// <summary>
    /// Streams a paginated list of roles from the Altinn Register service.
    /// </summary>
    /// <param name="subscriptionId">The subscriptionId for the actual role delegation events</param>
    /// <param name="nextPage">The URL of the next page, if paginated.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous enumerable of paginated <see cref="PartyModel"/> items.</returns>
    Task<IAsyncEnumerable<PlatformResponse<PageStream<RoleDelegationModel>>>> StreamRoles(string subscriptionId, string nextPage = null, CancellationToken cancellationToken = default);
}
