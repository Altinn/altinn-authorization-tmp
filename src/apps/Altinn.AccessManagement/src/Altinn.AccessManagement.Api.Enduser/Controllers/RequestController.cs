using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

[ApiController]
[Route("accessmanagement/api/v1/enduser/request")]
//// [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class RequestController(
    IRequestService requestService,
    IConnectionService connectionService,
    IEntityService entityService
    ) : ControllerBase
{
    private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
    {
        options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
        options.FilterFromEntityTypes = [];
        options.FilterToEntityTypes = [];
    };

    /// <summary>
    /// Get all requests for a party (as sender or receiver)
    /// </summary>
    [HttpGet]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<RequestDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequests([FromQuery] RequestEnduserQuery input, List<RequestStatus>? status, DateTimeOffset? after, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
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

        status = status == null ? new List<RequestStatus>() : status;

        var result = await requestService.GetRequests(fromId, toId, status, after, ct);
        return Ok(PaginatedResult.Create(result, null));
    }

    /// <summary>
    /// Create a package request on behalf of a party
    /// </summary>
    [HttpPost("package")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePackageRequest([FromQuery] RequestEnduserQuery input, [FromQuery] Guid packageId, CancellationToken ct = default)
    {
        var validFrom = Guid.TryParse(input.From, out var fromId);
        var validTo = Guid.TryParse(input.To, out var toId);

        if (!validFrom || !validTo)
        {
            return BadRequest("Both from and to must be specified");
        }

        if (packageId == Guid.Empty)
        {
            return BadRequest("packageId must be specified");
        }

        var result = await requestService.CreateRequestAssignmentPackage(fromId, toId, RoleConstants.Rightholder.Id, packageId, RequestStatus.Pending, ct);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(DtoMapper.Convert(result.Value));
    }

    /// <summary>
    /// Create a resource request on behalf of a party
    /// </summary>
    [HttpPost("resource")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateResourceRequest([FromQuery] RequestEnduserQuery input, [FromQuery] Guid resourceId, CancellationToken ct = default)
    {
        var validFrom = Guid.TryParse(input.From, out var fromId);
        var validTo = Guid.TryParse(input.To, out var toId);

        if (!validFrom || !validTo)
        {
            return BadRequest("Both from and to must be specified");
        }

        if (resourceId == Guid.Empty)
        {
            return BadRequest("resourceId must be specified");
        }

        var result = await requestService.CreateRequestAssignmentResource(fromId, toId, RoleConstants.Rightholder.Id, resourceId, RequestStatus.Pending, ct);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(DtoMapper.Convert(result.Value));
    }

    /// <summary>
    /// Accept (approve) a pending request — runs the same delegation logic as AddPackages/AddResourceRights
    /// </summary>
    [HttpPut("{id}/accept")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptRequest([FromRoute] Guid id, CancellationToken ct = default)
    {
        var existing = await requestService.GetRequest(id, ct);
        if (existing is null)
        {
            return NotFound();
        }

        return existing.RequestType switch
        {
            "package" => await AcceptPackageRequest(id, ct),
            "resource" => await AcceptResourceRequest(id, ct),
            _ => BadRequest($"Unknown request type: {existing.RequestType}")
        };
    }

    /// <summary>
    /// Confirm a draft request (transitions Draft → Pending)
    /// </summary>
    [HttpPut("{id}/confirm")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmRequest([FromRoute] Guid id, CancellationToken ct = default)
        => await UpdateRequestStatus(id, RequestStatus.Pending, ct);

    /// <summary>
    /// Reject a pending request
    /// </summary>
    [HttpPut("{id}/reject")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRequest([FromRoute] Guid id, CancellationToken ct = default)
        => await UpdateRequestStatus(id, RequestStatus.Rejected, ct);

    // Identical to AddPackages in ConnectionsController, but input comes from the RequestId
    private async Task<IActionResult> AcceptPackageRequest(Guid requestId, CancellationToken ct)
    {
        var request = await requestService.GetRequestAssignmentPackage(requestId, ct);

        var result = await connectionService.AddPackage(
            request.Assignment.FromId,
            request.Assignment.ToId,
            request.PackageId,
            ConfigureConnections,
            ct);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var updateResult = await requestService.UpdateRequestAssignmentPackage(requestId, RequestStatus.Approved, ct);
        if (updateResult.IsProblem)
        {
            return updateResult.Problem.ToActionResult();
        }

        return Ok(DtoMapper.Convert(request));
    }

    // Identical to AddResourceRights in ConnectionsController, but input comes from the RequestId.
    // Right keys are fetched automatically via ResourceDelegationCheck — all delegatable rights are granted.
    private async Task<IActionResult> AcceptResourceRequest(Guid requestId, CancellationToken ct)
    {
        var request = await requestService.GetRequestAssignmentResource(requestId, ct);

        var byId = AuthenticationHelper.GetPartyUuid(HttpContext);
        var by = await entityService.GetEntity(byId, ct);

        var delegationCheck = await connectionService.ResourceDelegationCheck(
            byId,
            request.Assignment.FromId,
            request.Resource.RefId,
            ConfigureConnections,
            cancellationToken: ct);

        if (delegationCheck.IsProblem)
        {
            return delegationCheck.Problem.ToActionResult();
        }

        var rightKeys = delegationCheck.Value.Rights
            .Where(r => r.Result)
            .Select(r => r.Right.Key)
            .ToList();

        var result = await connectionService.AddResource(
            request.Assignment.From,
            request.Assignment.To,
            request.Resource,
            new RightKeyListDto { DirectRightKeys = rightKeys },
            by,
            ConfigureConnections,
            ct);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var updateResult = await requestService.UpdateRequestAssignmentResource(requestId, RequestStatus.Approved, ct);
        if (updateResult.IsProblem)
        {
            return updateResult.Problem.ToActionResult();
        }

        return Ok(DtoMapper.Convert(request));
    }

    private async Task<IActionResult> UpdateRequestStatus(Guid id, RequestStatus status, CancellationToken ct)
    {
        var existing = await requestService.GetRequest(id, ct);
        if (existing is null)
        {
            return NotFound();
        }

        switch (existing.RequestType)
        {
            case "package":
                var packageResult = await requestService.UpdateRequestAssignmentPackage(id, status, ct);
                if (packageResult.IsProblem)
                {
                    return packageResult.Problem.ToActionResult();
                }

                return Ok(DtoMapper.Convert(packageResult.Value));

            case "resource":
                var resourceResult = await requestService.UpdateRequestAssignmentResource(id, status, ct);
                if (resourceResult.IsProblem)
                {
                    return resourceResult.Problem.ToActionResult();
                }

                return Ok(DtoMapper.Convert(resourceResult.Value));

            default:
                return BadRequest($"Unknown request type: {existing.RequestType}");
        }
    }
}
