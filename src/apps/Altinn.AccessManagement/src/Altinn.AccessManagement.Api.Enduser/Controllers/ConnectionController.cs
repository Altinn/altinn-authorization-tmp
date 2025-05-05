using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enduser.Services;
using Altinn.AccessMgmt.Core.Models;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for en user api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/connections")]
// [FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
// [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionController(IHttpContextAccessor accessor, IEnduserConnectionService connectionService) : ControllerBase
{
    private IHttpContextAccessor Accessor { get; } = accessor;

    private IEnduserConnectionService ConnectionService { get; } = connectionService;

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="from">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="to">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<PaginatedResult<AssignmentExternal>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignment([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken = default)
    {
        var userUuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserGetConnection(userUuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.GetAssignments(from.ConvertToUuid(userUuid), to.ConvertToUuid(userUuid), cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<Assignment>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAssignment([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserAddConnection(useruuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.AddAssignment(from.ConvertToUuid(useruuid), to.ConvertToUuid(useruuid), "rettighetshaver", cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result);
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserRemoveConnection(useruuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        problem = await ConnectionService.RemoveAssignment(from.ConvertToUuid(useruuid), to.ConvertToUuid(useruuid), "rettighetshaver", cascade, cancellationToken);
        if (problem is { })
        {
            problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    /// <param name="party">The GUID identifying the party the authenticated user is acting on behalf of.</param>
    /// <param name="from">The GUID identifying the party the authenticated user is acting for</param>
    /// <param name="to">The GUID identifying the target party to which the assignment should be created.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    [HttpGet("accesspackages")]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<PaginatedResult<ConnectionPackage>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, CancellationToken cancellationToken = default)
    {
        var partyUuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserGetConnection(partyUuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.GetPackages(from.ConvertToUuid(partyUuid), to.ConvertToUuid(partyUuid), cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost("accesspackages")]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<AssignmentPackage>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPackages([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, [FromQuery] Guid? packageId, [FromQuery] string packageUrn, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserAddConnection(useruuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        async Task<Result<AssignmentPackage>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(from.ConvertToUuid(useruuid), to.ConvertToUuid(useruuid), "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.AddPackage(from.ConvertToUuid(useruuid), to.ConvertToUuid(useruuid), "rettighetshaver", packageUrn, cancellationToken);
        }

        var result = await AddPackage();
        if (result.IsProblem)
        {
            result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete("accesspackages")]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages([FromQuery] string party, [FromQuery] string from, [FromQuery] string to, [FromQuery] Guid package, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserAddConnection(useruuid, party, from, to) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        problem = await ConnectionService.RemovePackage(from.ConvertToUuid(useruuid), to.ConvertToUuid(useruuid), "rettighetshaver", package, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }
}
