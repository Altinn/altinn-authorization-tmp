using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace Altinn.AccessManagement.Controllers;

/// <summary>
/// Controller for internal api operations for system user client delegation.
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/internal/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class SystemUserClientDelegationController : ControllerBase
{
    private readonly IConnectionRepository connectionRepository;
    private readonly IDelegationService delegationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemUserClientDelegationController"/> class.
    /// </summary>
    public SystemUserClientDelegationController(IConnectionRepository connectionRepository, IDelegationService delegationService)
    {
        this.connectionRepository = connectionRepository;
        this.delegationService = delegationService;
    }

    /// <summary>
    /// Post client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="request">Request Dto</param>
    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    public async Task<ActionResult> PostClientDelegation([FromQuery] Guid party, [FromBody] CreateSystemDelegationRequestDto request)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var delegation = await delegationService.CreateClientDelegation(request, userId, party);
        var res = await connectionRepository.GetExtended(delegation.Id);

        return Ok();
    }

    /// <summary>
    /// Delete client delegation
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="client">The client the authenticated user is removing access from</param>
    /// <param name="systemUser">The system user the authenticated user is removing client access to</param>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_WRITE)]
    public async Task<ActionResult> DeleteClientDelegation([FromQuery] Guid party, [FromQuery] Guid client, [FromQuery] Guid systemUser)
    {
        return await Task.FromResult(Ok());
    }

    /// <summary>
    /// Gets all client delegations for a given system user
    /// </summary>
    /// <param name="party">The party the authenticated user is performing client administration on behalf of</param>
    /// <param name="systemUser">The system user the authenticated user is delegating access to</param>
    /// <returns></returns>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_CLIENTDELEGATION_READ)]
    public async Task<ActionResult> GetClientDelegations([FromQuery] Guid party, [FromQuery] Guid systemUser)
    {
        var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
        if (userId == Guid.Empty)
        {
            return Unauthorized();
        }

        var filter = connectionRepository.CreateFilterBuilder();
        filter.Equal(t => t.ToId, systemUser);
        filter.Equal(t => t.FacilitatorId, party);
        var res = await connectionRepository.GetExtended(filter);

        return Ok(res);
    }
}
