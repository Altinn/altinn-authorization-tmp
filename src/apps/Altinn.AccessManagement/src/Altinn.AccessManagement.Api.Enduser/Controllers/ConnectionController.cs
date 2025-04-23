using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Mappers;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Filters;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for en user api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/access/connection")]
[FeatureGate(AccessManagementEnduserFeatureFlags.ControllerAccessParties)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionController(IHttpContextAccessor accessor, IConnectionService connectionService) : ControllerBase
{
    private IHttpContextAccessor Accessor { get; } = accessor;

    private IConnectionService ConnectionService { get; } = connectionService;

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="from">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="to">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType<AssignmentExternal>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnection([FromQuery] Guid party, [FromQuery] Guid? from, [FromQuery] Guid? to, CancellationToken cancellationToken = default)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return BadRequest();
        }

        var partyUuid = Accessor.GetPartyUuid();
        var audit = new ChangeRequestOptions()
        {
            ChangedBy = partyUuid,
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (from.HasValue && to.HasValue)
        {
            return Ok(await connectionService.GetSpecific(from.Value, to.Value));
        }

        if (from.HasValue)
        {
            return Ok(await connectionService.GetRecived(to.Value));
        }

        if (to.HasValue)
        {
            return Ok(await connectionService.GetGiven(to.Value));
        }
        
        return BadRequest();
    }

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="from">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="to">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet]
    [Route("packages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType<AssignmentExternal>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnectionPackages([FromQuery] Guid party, [FromQuery] Guid? from, [FromQuery] Guid? to, CancellationToken cancellationToken = default)
    {
        if (!from.HasValue && !to.HasValue)
        {
            return BadRequest();
        }

        var partyUuid = Accessor.GetPartyUuid();
        var audit = new ChangeRequestOptions()
        {
            ChangedBy = partyUuid,
            ChangedBySystem = AuditDefaults.EnduserApi
        };

        if (from.HasValue && to.HasValue)
        {
            return Ok(await connectionService.GetSpecific(from.Value, to.Value));
        }

        if (from.HasValue)
        {
            return Ok(await connectionService.GetRecived(to.Value));
        }

        if (to.HasValue)
        {
            return Ok(await connectionService.GetGiven(to.Value));
        }

        return BadRequest();
    }
}
