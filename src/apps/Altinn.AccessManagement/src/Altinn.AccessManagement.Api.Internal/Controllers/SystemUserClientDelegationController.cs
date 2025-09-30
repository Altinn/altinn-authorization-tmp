﻿using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClientDto = Altinn.Authorization.Api.Contracts.AccessManagement.ClientDto;
using CreateSystemDelegationRequestDto = Altinn.Authorization.Api.Contracts.AccessManagement.CreateSystemDelegationRequestDto;

namespace Altinn.AccessManagement.Api.Internal.Controllers;

/// <summary>
/// Controller for internal api operations for system user client delegation.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/internal/[controller]")]
[ApiExplorerSettings(IgnoreApi = false)]
public class SystemUserClientDelegationController(
        IAssignmentService assignmentService
        ,IConnectionService connectionService
        ,IDelegationService delegationService
    ) : ControllerBase
{
    private readonly string[] validClientRoles = ["regnskapsforer", "revisor", "forretningsforer", "rettighetshaver"];

    /// <summary>
    /// Gets all clients for a given facilitator
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="roles"> The list of role codes to filter the connections by</param>
    /// <param name="packages"> The list of package identifiers to filter the connections by</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>List of Clients<seealso cref="ClientDto"/></returns>
    [HttpGet("clients")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_READ)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetClients([FromQuery] Guid party, [FromQuery] string[] roles = null, [FromQuery] string[] packages = null, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        if (roles != null && roles.Length > 0)
        {
            var invalidRoles = roles.Where(role => !validClientRoles.Contains(role));
            if (invalidRoles.Any())
            {
                return BadRequest($"Invalid role filter: '{string.Join(",", invalidRoles)}'. Valid Client roles are: '{string.Join(", ", validClientRoles)}'");
            }
        }
        else
        {
            roles = validClientRoles;
        }

        if (packages == null || packages.Length == 0)
        {
            packages = [];
        }

        var clients = await assignmentService.GetClients(party, roles, packages, cancellationToken);

        return Ok(clients);
    }

    /// <summary>
    /// Gets all client delegations for a given system user
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="systemUser">The system user the authenticated user is delegating access to</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns><seealso cref="SystemUserClientConnectionDto"/>List of connections</returns>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_READ)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_READ)]
    public async Task<ActionResult<IEnumerable<SystemUserClientConnectionDto>>> GetClientDelegations([FromQuery] Guid party, [FromQuery] Guid systemUser, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        return Ok(await connectionService.GetConnectionsToAgent(viaId: party, toId: systemUser, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Post client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="request">Request Dto</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns><seealso cref="CreateDelegationResponseDto"/>List of delegation responses</returns>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)] // ToDo: Change to new Audit Claim when available
    public async Task<ActionResult<CreateDelegationResponseDto>> PostClientDelegation([FromQuery] Guid party, [FromBody] CreateSystemDelegationRequestDto request, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var delegations = await delegationService.CreateClientDelegation(request, party, cancellationToken);

        // Remark: Kan ikke garantere at det KUN er delegeringer som er opprettet i denne handlingen som blir returnert.
        return Ok(delegations);
    }

    /// <summary>
    /// Delete client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="delegationId">The delegation identifier</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    [HttpDelete]
    [Route("deletedelegation")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)] // ToDo: Change to new Audit Claim when available
    public async Task<ActionResult> DeleteDelegation([FromQuery] Guid party, [FromQuery] Guid delegationId, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        /*
         - [X] Delegation exists
         - [X] if party is facilitator for delegation
        */

        var delegation = await delegationService.GetDelegation(delegationId, cancellationToken);
        if (delegation == null)
        {
            return BadRequest("Delegation not found");
        }

        if (delegation.FacilitatorId != party)
        {
            return BadRequest("Party does not match delegation facilitator");
        }

        var clientAssignmentToFacilitator = await assignmentService.GetAssignment(delegation.FromId, cancellationToken);
        if (!clientAssignmentToFacilitator.ToId.Equals(party))
        {
            return BadRequest("Party does not match delegation assignments");
        }

        var result = await delegationService.DeleteDelegation(delegation.Id, cancellationToken);

        if (result != null)
        {
            return result.ToActionResult();
        }

        return Ok();
    }

    /// <summary>
    /// Delete client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="assignmentId">The assignment identifier</param>
    /// <param name="cascade">If true; dependent rows in the database will be deleted</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    [HttpDelete]
    [Route("deleteassignment")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    public async Task<ActionResult> DeleteAssignment([FromQuery] Guid party, [FromQuery] Guid assignmentId, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        /*
         - [X] Assignment exists
         - [X] Assignment connected to party
         - [X] Assignment role is owned by Digdir
         - [X] Assignment not connected to any Delegation(or cascade = true)
         - [X] Temp: Only 'agent' role - Get this from queryparam future
        */

        string roleIdentifier = "agent"; 

        var assignment = await assignmentService.GetAssignment(assignmentId);
        if (assignment == null)
        {
            return BadRequest("Assignment not found");
        }

        if (!assignment.FromId.Equals(party))
        {
            return BadRequest("Assignment not from party");
        }

        if (!assignment.Role.Code.Equals(roleIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            return Problem($"You cannot removed assignments with this role '{assignment.Role.Code}', only '{roleIdentifier}'");
        }

        var result = await assignmentService.DeleteAssignment(assignment.Id, cascade, cancellationToken);

        if (result != null)
        {
            return Problem("Assignment is active in one or more delegations and cascadeflag is false.");
        }

        return Ok();
    }

    /// <summary>
    /// Delete client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="agentId">The agent/system party identifier</param>
    /// <param name="cascade">If true; dependent rows in the database will be deleted</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    [HttpDelete]
    [Route("deleteagentassignment")]
    [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_CLIENTDELEGATION_WRITE)]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    public async Task<ActionResult> DeleteAgentAssignment([FromQuery] Guid party, [FromQuery] Guid agentId, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var result = await assignmentService.DeleteAssignment(party, agentId, "agent", cascade, cancellationToken);

        if (result != null)
        {
            return result.ToActionResult();
        }

        return Ok();
    }
}
