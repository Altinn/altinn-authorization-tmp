using System.Diagnostics;
using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Enduser.Services;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Swashbuckle.AspNetCore.Filters;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for en user api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/connections")]
// [FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
// [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionController(IHttpContextAccessor accessor, IEnduserConnectionService connectionService, IDbAudit audit) : ControllerBase
{
    private IHttpContextAccessor Accessor { get; } = accessor;

    private IEnduserConnectionService ConnectionService { get; } = connectionService;

    private IDbAudit Audit { get; } = audit;

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [SwaggerRequestExample(typeof(PagingInput), typeof(PagingInput))]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<AssignmentExternal>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssignment([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        var userUuid = Audit.Value;
        if (ValidationRules.EnduserGetConnection(userUuid.ChangedBy, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.GetAssignments(connection.From.ConvertToUuid(userUuid.ChangedBy), connection.To.ConvertToUuid(userUuid.ChangedBy), cancellationToken);
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
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<Assignment>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAssignment([FromQuery] ConnectionInput connection, CancellationToken cancellationToken = default)
    {
        var audit = Audit.Value;
        if (ValidationRules.EnduserAddConnection(audit.ChangedBy, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.AddAssignment(connection.From.ConvertToUuid(audit.ChangedBy), connection.To.ConvertToUuid(audit.ChangedBy), "rettighetshaver", cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment([FromQuery] ConnectionInput connection, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        var audit = Audit.Value;
        if (ValidationRules.EnduserRemoveConnection(audit.ChangedBy, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        problem = await ConnectionService.RemoveAssignment(connection.From.ConvertToUuid(audit.ChangedBy), connection.To.ConvertToUuid(audit.ChangedBy), "rettighetshaver", cascade, cancellationToken);
        if (problem is { })
        {
            problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("accesspackages")]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<PaginatedResult<ConnectionPackage>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        var partyUuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserGetConnection(partyUuid, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var result = await ConnectionService.GetPackages(connection.From.ConvertToUuid(partyUuid), connection.To.ConvertToUuid(partyUuid), cancellationToken);
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
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter<AuthorizePartyUuidClaimFilter>]
    [ProducesResponseType<AssignmentPackage>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string packageUrn, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserAddConnection(useruuid, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        async Task<Result<AssignmentPackage>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(connection.From.ConvertToUuid(useruuid), connection.To.ConvertToUuid(useruuid), "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.AddPackage(connection.From.ConvertToUuid(useruuid), connection.To.ConvertToUuid(useruuid), "rettighetshaver", packageUrn, cancellationToken);
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
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    // [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    // [ServiceFilter(typeof(AuthorizePartyUuidClaimFilter))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages([FromQuery] ConnectionInput connection, [FromQuery] Guid package, CancellationToken cancellationToken = default)
    {
        var useruuid = Accessor.GetPartyUuid();
        if (ValidationRules.EnduserAddConnection(useruuid, connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        problem = await ConnectionService.RemovePackage(connection.From.ConvertToUuid(useruuid), connection.To.ConvertToUuid(useruuid), "rettighetshaver", package, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }
}
