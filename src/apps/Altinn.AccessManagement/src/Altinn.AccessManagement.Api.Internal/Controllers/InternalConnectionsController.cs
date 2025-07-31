using System.Net.Mime;
using Altinn.AccessManagement.Api.Internal.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Internal.Services;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers;

/// <summary>
/// Controller for managing direct assigment of packages for system users.  
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/internal/connections")]
[FeatureGate(AccessManagementInternalFeatureFlags.ControllerConnections)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class InternalConnectionsController(IInternalConnectionService connectionService) : ControllerBase
{
    private IInternalConnectionService ConnectionService { get; } = connectionService;

    /// <summary>
    /// Get connections between organizations and systemusers.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<CompactRelationDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnections([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.ReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        var result = await ConnectionService.Get(fromUuid, toUuid, cancellationToken: cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Creates "rettighetshaver" relation between an organization and systemuser.
    /// </summary>
    [HttpPost]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.InternalApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<Assignment>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAssignment([FromQuery] ConnectionInput connection, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.AddConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        var result = await ConnectionService.AddAssignment(fromUuid, toUuid, "rettighetshaver", cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes "rettighetshaver" relation between an organization and systemuser.
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.InternalApiStr)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment([FromQuery] ConnectionInput connection, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.RemoveConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        problem = await ConnectionService.RemoveAssignment(fromUuid, toUuid, "rettighetshaver", cascade, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Lists all packages assigned from to / systemuser and organization. 
    /// </summary>
    [HttpGet("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.InternalApiStr)]
    [ProducesResponseType<PaginatedResult<PackagePermission>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.ReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        var result = await ConnectionService.GetPackages(fromUuid, toUuid, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(PaginatedResult.Create(result.Value, null));
    }

    /// <summary>
    /// Assigns package to system user from organization. 
    /// </summary>
    [HttpPost("accesspackages")]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.InternalApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentPackage>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.AddConnectionPackage(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<Result<AssignmentPackage>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(fromUuid, toUuid, "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.AddPackage(fromUuid, toUuid, "rettighetshaver", package, cancellationToken);
        }

        var result = await AddPackage();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes package given to system user from an organization. 
    /// </summary>
    [HttpDelete("accesspackages")]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.InternalApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (InternalValidationRules.RemoveConnectionPacakge(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        Guid.TryParse(connection.From, out var fromUuid);
        Guid.TryParse(connection.To, out var toUuid);
        async Task<ValidationProblemInstance> RemovePackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.RemovePackage(fromUuid, toUuid, "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.RemovePackage(fromUuid, toUuid, "rettighetshaver", package, cancellationToken);
        }

        problem = await RemovePackage();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }
}
