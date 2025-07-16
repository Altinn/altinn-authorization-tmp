using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Mappers;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Services;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Connection;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

/// <summary>
/// Controller for en user api operations for connections
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/enduser/connections")]
[FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class ConnectionsController(ConnectionAdapter connectionAdapter) : ControllerBase
{
    private ConnectionAdapter ConnectionService { get; } = connectionAdapter;

    /// <summary>
    /// Get connections between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [ProducesResponseType<PaginatedResult<CompactConnectionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetConnections([FromQuery] ConnectionInputDto connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        var result = await ConnectionService.Get(fromId, toId, cancellationToken: cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var dtos = LegacyBridgeMappers.ToDto(result.Value);
        return Ok(PaginatedResult.Create(dtos, null));
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddAssignment([FromQuery] ConnectionInputDto connection, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserAddConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        var result = await ConnectionService.AddAssignment(fromId!.Value, toId!.Value, "rettighetshaver", cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var dto = LegacyBridgeMappers.ToDto(result.Value);
        return Ok(dto);
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveAssignment([FromQuery] ConnectionInputDto connection, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserRemoveConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        problem = await ConnectionService.RemoveAssignment(fromId!.Value, toId!.Value, "rettighetshaver", cascade, cancellationToken);
        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Creates an assignment between the authenticated user's selected party and the specified target party.
    /// </summary>
    [HttpGet("accesspackages")]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [ProducesResponseType<PaginatedResult<PackagePermissionDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackages([FromQuery] ConnectionInputDto connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserReadConnection(connection.Party, connection.From, connection.To) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        var result = await ConnectionService.GetPackages(fromId, toId, cancellationToken);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var dtos = LegacyBridgeMappers.ToDto(result.Value);
        return Ok(PaginatedResult.Create(dtos, null));
    }

    /// <summary>
    /// Add package to connection (assignment or delegation)
    /// </summary>
    [HttpPost("accesspackages")]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPackages([FromQuery] ConnectionInputDto connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserAddConnectionPackage(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        async Task<Result<AssignmentPackage>> AddPackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.AddPackage(fromId!.Value, toId!.Value, "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.AddPackage(fromId!.Value, toId!.Value, "rettighetshaver", package, cancellationToken);
        }

        var result = await AddPackage();
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var dto = LegacyBridgeMappers.ToDto(result.Value);
        return Ok(dto);
    }

    /// <summary>
    /// Remove package from connection (assignment or delegation)
    /// </summary>
    [HttpDelete("accesspackages")]
    [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemovePackages([FromQuery] ConnectionInputDto connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
    {
        if (EnduserValidationRules.EnduserRemoveConnectionPacakge(connection.Party, connection.From, connection.To, packageId, package) is var problem && problem is { })
        {
            return problem.ToActionResult();
        }

        var coreInput = ConnectionMappers.ToCore(connection);
        var fromId = Guid.TryParse(coreInput.From, out var fromGuid) ? fromGuid : null as Guid?;
        var toId = Guid.TryParse(coreInput.To, out var toGuid) ? toGuid : null as Guid?;
        async Task<ValidationProblemInstance> RemovePackage()
        {
            if (packageId.HasValue)
            {
                return await ConnectionService.RemovePackage(fromId!.Value, toId!.Value, "rettighetshaver", packageId.Value, cancellationToken);
            }

            return await ConnectionService.RemovePackage(fromId!.Value, toId!.Value, "rettighetshaver", package, cancellationToken);
        }

        problem = await RemovePackage();

        if (problem is { })
        {
            return problem.ToActionResult();
        }

        return NoContent();
    }
}
