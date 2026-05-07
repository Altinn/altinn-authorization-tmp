using Altinn.Authorization.Api.Contracts.Authorization;

namespace Altinn.AccessMgmt.Core.Services.Contracts;

/// <summary>
/// Service for making authorization decisions based on XACML requests.
/// Handles context enrichment, policy evaluation, and delegation resolution.
/// </summary>
public interface IAuthorizationDecisionService
{
    /// <summary>
    /// Authorizes an external XACML JSON request and returns the authorization response.
    /// </summary>
    /// <param name="request">The authorization request containing XACML JSON categories.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The authorization response with decisions for each request.</returns>
    Task<AuthorizationResponseDto> AuthorizeAsync(AuthorizationRequestDto request, CancellationToken cancellationToken = default);
}
