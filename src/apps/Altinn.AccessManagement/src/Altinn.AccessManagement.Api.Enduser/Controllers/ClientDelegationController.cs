using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Utils;
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
    [HttpGet("clients")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetClients(
        [FromQuery(Name = "party")][Required] Guid party,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var result = await clientDelegationService.GetClientsAsync(party, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    [HttpGet("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [ProducesResponseType<PaginatedResult<ConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAgents(
        [FromQuery(Name = "party")][Required] Guid party,
        [FromQuery, FromHeader] PagingInput paging,
        CancellationToken cancellationToken = default)
    {
        var result = await clientDelegationService.GetAgentsAsync(party, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    [HttpPost("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAgent(
        [FromQuery(Name = "party")][Required] Guid party,
        [FromQuery(Name = "to")][Required] Guid to,
        CancellationToken cancellationToken = default)
    {
        var result = await clientDelegationService.AddAgent(party, to, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    [HttpDelete("agents")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAgent(
        [FromQuery(Name = "party")][Required] Guid party,
        [FromQuery(Name = "to")][Required] Guid to,
        CancellationToken cancellationToken = default)
    {
        await clientDelegationService.RemoveAgent(party, to, cancellationToken);
        return NoContent();
    }

    [HttpGet("agents/accesspackages")]
    public async Task<IActionResult> GetClientAccessPackage()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpPost("agents/accesspackages")]
    public async Task<IActionResult> AddClientAccessPackage()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }

    [HttpDelete("agents/accesspackages")]
    public async Task<IActionResult> DeleteClientAccessPackage()
    {
        return StatusCode(StatusCodes.Status501NotImplemented);
    }
}
