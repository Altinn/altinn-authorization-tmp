using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers;

[ApiController]
[Route("accessmanagement/api/v1/serviceowner/request")]
//[Authorize(Policy = AuthzConstants.SCOPE_SYSTEMOWNER)]
public class RequestController : ControllerBase
{
    [HttpGet("_meta/status")]
    [ProducesResponseType(typeof(IReadOnlyCollection<RequestStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetaStatuses(CancellationToken cancellationToken = default)
    {
        return Ok(RequestStatusMapping.All);
    }

    [HttpGet("{id}/status")]
    //[Authorize(Policy = AuthzConstants.SYSTEMOWNER_REQUEST_GET)]
    [ProducesResponseType<RequestStatusDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequestStatus([FromQuery] Guid id, [FromQuery] RequestQueryInput input, CancellationToken cancellationToken = default)
    {
        return Ok();
    }

    #region Packages
    [HttpGet("package")]
    //[Authorize(Policy = AuthzConstants.SYSTEMOWNER_REQUEST_GET)]
    [ProducesResponseType<RequestPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindPackageRequests([FromQuery] RequestQueryInput input, CancellationToken cancellationToken = default)
    {
        return Ok();
    }

    [HttpPost("package")]
    //[Authorize(Policy = AuthzConstants.SYSTEMOWNER_REQUEST_POST)]
    [ProducesResponseType<RequestPackageDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestPackage([FromBody] RequestPackageInput input, CancellationToken cancellationToken = default)
    {
        return Accepted();
    }
    #endregion

    #region Resources
    [HttpGet("resource")]
    //[Authorize(Policy = AuthzConstants.SYSTEMOWNER_REQUEST_GET)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindResourceRequests([FromQuery] RequestQueryInput input, CancellationToken cancellationToken = default)
    {
        return Ok();
    }

    [HttpPost("resource")]
    //[Authorize(Policy = AuthzConstants.SYSTEMOWNER_REQUEST_POST)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestResource([FromBody] RequestResourceInput input, CancellationToken cancellationToken = default)
    {
        return Accepted();
    }
    #endregion
}
