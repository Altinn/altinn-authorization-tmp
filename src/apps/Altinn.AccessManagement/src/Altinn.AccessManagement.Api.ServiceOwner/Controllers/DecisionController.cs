using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.Authorization.Api.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers;

/// <summary>
/// Controller for authorization decisions.
/// Provides the authorize endpoint for service owners to check access.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/serviceowner")]
public class DecisionController(IAuthorizationDecisionService authorizationDecisionService) : ControllerBase
{
    /// <summary>
    /// Authorize endpoint for service owners.
    /// Evaluates an XACML JSON authorization request and returns the decision.
    /// </summary>
    /// <param name="request">The authorization request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    [HttpPost("authorize")]
    [Authorize(Policy = AuthzConstants.POLICY_AUTHORIZE)]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<AuthorizationResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthorizationResponseDto>> Authorize(
        [FromBody] AuthorizationRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await authorizationDecisionService.AuthorizeAsync(request, cancellationToken);
        return Ok(response);
    }
}
