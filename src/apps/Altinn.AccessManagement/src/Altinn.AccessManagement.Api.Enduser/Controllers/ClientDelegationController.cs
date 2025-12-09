using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

[ApiController]
[Route("accessmanagement/api/v1/enduser/clientdelegations")]
[FeatureGate(AccessMgmtFeatureFlags.EnduserControllerClientDelegation)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ClientDelegationController(IClientDelegationService clientDelegationService) : ControllerBase
{
    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("clients")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClients(
        [FromQuery] PartyInput party,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var partyUuid = Guid.Parse(party.Party);
        var result = await clientDelegationService.GetClientsAsync(partyUuid, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnections(
        [FromQuery] PartyInput party,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var partyUuid = Guid.Parse(party.Party);
        var result = await clientDelegationService.GetClientsAsync(partyUuid, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpPost("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAgent(
        [FromQuery] PartyToInput party,
        CancellationToken cancellationToken = default)
    {
        var partyUuid = Guid.Parse(party.Party);
        var toUuid = Guid.Parse(party.To);
        var result = await clientDelegationService.AddAgent(partyUuid, toUuid, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpDelete("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAgent(
        [FromQuery] PartyToInput party,
        CancellationToken cancellationToken = default)
    {
        var partyUuid = Guid.Parse(party.Party);
        var toUuid = Guid.Parse(party.To);
        await clientDelegationService.RemoveAgent(partyUuid, toUuid, cancellationToken);
        return NoContent();
    }
}
