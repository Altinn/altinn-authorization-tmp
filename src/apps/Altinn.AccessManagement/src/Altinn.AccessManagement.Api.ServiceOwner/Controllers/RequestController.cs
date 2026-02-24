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

/// <summary>
/// Dto for creating a new request for access package
/// </summary>
public class RequestQueryInput
{
    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "from")]
    public string From { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "to")]
    public string To { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    [FromQuery(Name = "party")]
    public string Party { get; set; }
}

/// <summary>
/// Dto for creating a new request for access package
/// </summary>
public class RequestInput
{
    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Urn describing the party
    /// </summary>
    public string Party { get; set; }
}

/// <summary>
/// Dto for creating a new request for access package
/// </summary>
public class RequestPackageInput : RequestInput
{
    /// <summary>
    /// Urn describing the package
    /// </summary>
    public PackageRefrenceDto Package { get; set; }
}

/// <summary>
/// Dto for creating a new request for resource
/// </summary>
public class RequestResourceInput : RequestInput
{
    /// <summary>
    /// Urn describing the package
    /// </summary>
    public ResourceRefrenceDto Resource { get; set; }
}

/// <summary>
/// Resource reference
/// </summary>
public class ResourceRefrenceDto
{
    /// <summary>
    /// Urn identifying the resource
    /// </summary>
    public string Resource { get; set; }
}

/// <summary>
/// Access package reference
/// </summary>
public class PackageRefrenceDto
{
    /// <summary>
    /// Urn identifying the package
    /// </summary>
    public string Package { get; set; }
}

/// <summary>
/// Response dto for resource request
/// </summary>
public class RequestResourceDto
{
    /// <summary>
    /// Request id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Request status (e.g. pending, approved, rejected, withdrawn)
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Party that is requested to grant access
    /// </summary>
    public EntityDto From { get; set; }

    /// <summary>
    /// Party that is access is requested for
    /// </summary>
    public EntityDto To { get; set; }

    /// <summary>
    /// Resource that is requested
    /// </summary>
    public ResourceRefrenceDto Resource { get; set; }
}

/// <summary>
/// Response dto for access package request
/// </summary>
public class RequestPackageDto
{
    /// <summary>
    /// Request id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Request status (e.g. pending, approved, rejected, withdrawn)
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Party that is requested to grant access
    /// </summary>
    public EntityDto From { get; set; }

    /// <summary>
    /// Party that is access is requested for
    /// </summary>
    public EntityDto To { get; set; }

    /// <summary>
    /// Access package that is requested
    /// </summary>
    public PackageRefrenceDto Package { get; set; }
}

/// <summary>
/// Party entity refrence
/// </summary>
public class EntityDto
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Type of entity (e.g. organization, person, systemuser)
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Variant of entity (e.g. for organization: AS, ENK, DA)
    /// </summary>
    public string Variant { get; set; }
}
