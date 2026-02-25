using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers;

/// <summary>
/// Request access
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/serviceowner/delegationrequests")]
public class RequestController : ControllerBase
{
    /// <summary>
    /// Get valid urn prefixes for party identification
    /// </summary>
    [HttpGet("_meta/urns/party")]
    [ProducesResponseType(typeof(IReadOnlyCollection<RequestStatus>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValidUrns(CancellationToken cancellationToken = default)
    {
        return Ok(ValidUrns);
    }

    private static string[] ValidUrns =>
    [
        "urn:altinn:person:identifier-no",
        "urn:altinn:organization:identifier-no",
        "urn:altinn:systemuser:uuid",
        "urn:altinn:party:uuid"
    ];

    /// <summary>
    /// Get resourc requests for a given party
    /// </summary>
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindResourceRequests([FromQuery] RequestQueryInput input, CancellationToken cancellationToken = default)
    {
        return Ok();
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost("resource")]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestResource([FromBody] RequestResourceInput input, CancellationToken cancellationToken = default)
    {        
        return Accepted("deeplink.....");
    }
}
