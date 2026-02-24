using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.AccessList;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

[ApiController]
[Route("accessmanagement/api/v1/enduser/request")]
[FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class RequestController(
    IRequestService requestService
    ) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<RequestDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequests([FromQuery] RequestInput input, List<RequestStatus>? status, DateTimeOffset? after, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
    {
        var validParty = Guid.TryParse(input.Party, out var partyId);
        var validFrom = Guid.TryParse(input.From, out var fromId);
        var validTo = Guid.TryParse(input.To, out var toId);

        if (!validFrom && !validTo)
        {
            return BadRequest("Either from or to must be specified");
        }

        if (partyId != fromId && partyId != toId)
        {
            return BadRequest("Party must be either from or to");
        }

        var result = await requestService.GetRequests(fromId, toId, partyId, status, after, ct);
        return Ok(PaginatedResult.Create(result, null));
    }

    [HttpPost]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePackageRequest([FromQuery] RequestInput input, Guid packageId, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
    {
        var validParty = Guid.TryParse(input.Party, out var partyId);
        var validFrom = Guid.TryParse(input.From, out var fromId);
        var validTo = Guid.TryParse(input.To, out var toId);

        var result = await requestService.GetRequests(fromId, toId, partyId, null, null, ct);
        return Ok(PaginatedResult.Create(result, null));
    }

    [HttpPut("/{id}/accept")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AcceptRequest([FromQuery] RequestInput input, [FromQuery] Guid requestId, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
    {
        var validParty = Guid.TryParse(input.Party, out var partyId);
        var validFrom = Guid.TryParse(input.From, out var fromId);
        var validTo = Guid.TryParse(input.To, out var toId);

        var requests = await requestService.GetRequest(requestId, ct);
        if (requests is not { })
        {
            return NotFound();
        }

        RequestDto result;

        await requestService.UpdateRequestAssignmentPackage(requestId, RequestStatus.Approved);

        return Ok();
    }
}

/// <summary>
/// Input for request controller.
/// </summary>
public class RequestInput
{
    /// <summary>
    /// making request on behalf of.
    /// </summary>
    [FromQuery(Name = "party")]
    [SwaggerSchema(Description = "party", Format = "<me, uuid>")]
    public string Party { get; set; }

    /// <summary>
    /// from party
    /// </summary>
    [FromQuery(Name = "from")]
    [SwaggerSchema(Description = "from", Format = "<me, all | blank, uuid>")]
    public string From { get; set; }

    /// <summary>
    /// to party
    /// </summary>
    [FromQuery(Name = "to")]
    [SwaggerSchema(Description = "to", Format = "<me, all | blank, uuid>")]
    public string To { get; set; }
}
