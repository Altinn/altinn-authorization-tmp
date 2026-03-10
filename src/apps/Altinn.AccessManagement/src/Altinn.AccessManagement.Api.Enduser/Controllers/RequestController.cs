using System.Net.Mime;
using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers;

[ApiController]
[Route("accessmanagement/api/v1/enduser/request")]
[Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
public class RequestController(
    IRequestService requestService,
    IConnectionService connectionService,
    IResourceService resourceService,
    IPackageService packageService,
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

    [HttpGet("sent")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<RequestDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSentRequests([FromQuery] Guid party, [FromQuery] Guid? to, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
    {
        var validStatus = new List<RequestStatus>()
        {
            RequestStatus.Draft,
            RequestStatus.Pending,
            RequestStatus.Approved,
            RequestStatus.Rejected,
            RequestStatus.Withdrawn
        };

        var afterTime = DateTimeOffset.UtcNow.AddDays(-30);

        var result = await requestService.GetRequests(fromId: party, toId: to, status: null, after: afterTime, ct);

        return Ok(PaginatedResult.Create(result, null));
    }

    [HttpGet("received")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [ProducesResponseType<PaginatedResult<RequestDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetReceivedRequests([FromQuery] Guid party, [FromQuery] Guid? from, [FromQuery, FromHeader] PagingInput paging, CancellationToken ct = default)
    {
        var validStatus = new List<RequestStatus>()
        {
            RequestStatus.Draft,
            RequestStatus.Pending,
            RequestStatus.Approved,
            RequestStatus.Rejected,
            RequestStatus.Withdrawn
        };

        var afterTime = DateTimeOffset.UtcNow.AddDays(-30);

        var result = await requestService.GetRequests(fromId: from, toId: party, status: null, after: afterTime, ct);
        return Ok(PaginatedResult.Create(result, null));
    }

    /// <summary>
    /// Create request on behalf of a party
    /// </summary>
    [HttpPost]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRequest([FromQuery] Guid party, [FromQuery] Guid to, [FromBody]CreateRequestInput input, CancellationToken ct = default)
    {
        /*
        Person1 vil be FirmaA om en rettighet, derfor er to = FirmaA i queryparam. 
        Men da blir Assignment.From = FirmaA og Assignment.To = Party (Person1)
        GLHF
        */

        var connections = await connectionService.GetConnectionsFromOthers(partyId: to, fromId: party);
        if (connections == null || !connections.Any())
        {
            return Forbid();
        }

        var resource = input.Resource is { } ? await resourceService.GetResource(input.Resource, ct) : null;
        var package = input.Package is { } ? await packageService.GetPackage(input.Package, ct) : null;
        var role = RoleConstants.Rightholder;
        var status = RequestStatus.Pending;

        var result = await requestService.CreateRequest(
                new CreateRequestDto()
                {
                    From = to, // YES, this is correct
                    To = party,
                    Role = role.Id,
                    Status = status,
                    Resource = resource?.Id,
                    Package = package?.Id,
                },
                ct
            );
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Confirm a draft request (transitions Draft → Pending)
    /// </summary>
    [HttpPut("sent/confirm")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmRequest([FromQuery] Guid id, CancellationToken ct = default)
    {
        return await UpdateRequestStatus(id, RequestStatus.Pending, ct);
    }

    /// <summary>
    /// Withdraw a request
    /// </summary>
    [HttpPut("sent/withdraw")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> WithdrawRequest([FromQuery] Guid id, CancellationToken ct = default)
    {
        return await UpdateRequestStatus(id, RequestStatus.Withdrawn, ct);
    }

    /// <summary>
    /// Reject a pending request
    /// </summary>
    [HttpPut("received/reject")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectRequest([FromQuery] Guid party, [FromQuery] Guid id, CancellationToken ct = default)
    {
        return await UpdateRequestStatus(id, RequestStatus.Rejected, ct);
    }

    /// <summary>
    /// Approve a pending request — runs the same delegation logic as AddPackages/AddResourceRights
    /// </summary>
    [HttpPut("received/approve")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
    [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveRequest([FromQuery] Guid id, CancellationToken ct = default)
    {
        var existing = await requestService.GetRequest(id, ct);
        if (existing is null)
        {
            return NotFound();
        }

        return existing.Resource is { } ? await ApproveResourceRequest(id, ct) : await ApprovePackageRequest(id, ct);
    }

    private async Task<IActionResult> ApprovePackageRequest(Guid requestId, CancellationToken ct)
    {
        var request = await requestService.GetRequest(requestId, ct);

        var result = await connectionService.AddPackage(
            request.Connection.From.Id,
            request.Connection.To.Id,
            request.Package.Id.Value,
            ConfigureConnections,
            ct);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var updateResult = await requestService.UpdateRequest(requestId, RequestStatus.Approved, ct);
        if (updateResult.IsProblem)
        {
            return updateResult.Problem.ToActionResult();
        }

        return Ok(request);
    }

    private async Task<IActionResult> ApproveResourceRequest(Guid requestId, CancellationToken ct)
    {
        var request = await requestService.GetRequest(requestId, ct);

        var byId = AuthenticationHelper.GetPartyUuid(HttpContext);
        var by = await entityService.GetEntity(byId, ct);

        var delegationCheck = await connectionService.ResourceDelegationCheck(
            byId,
            request.Connection.From.Id,
            request.Resource.Urn,
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

        if (!rightKeys.Any())
        {
            return Forbid("Missing rights to give");
        }

        var from = await entityService.GetEntity(request.Connection.From.Id, ct);
        var to = await entityService.GetEntity(request.Connection.To.Id, ct);
        var resource = await resourceService.GetResource(request.Resource.Id.Value, ct);

        var result = await connectionService.AddResource(
            from,
            to,
            resource,
            new RightKeyListDto { DirectRightKeys = rightKeys },
            by,
            ConfigureConnections,
            ct);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        var updateResult = await requestService.UpdateRequest(requestId, RequestStatus.Approved, ct);
        if (updateResult.IsProblem)
        {
            return updateResult.Problem.ToActionResult();
        }

        return Ok(request);
    }

    private async Task<IActionResult> UpdateRequestStatus(Guid id, RequestStatus status, CancellationToken ct)
    {
        var result = await requestService.UpdateRequest(id, status, ct);
        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Ok(result.Value);
    }

    private async Task<Entity> GetEntityByUrn(string urn, CancellationToken ct = default)
    {
        var urnSegments = urn.Split(":");
        var urnSuffix = urnSegments.Last();
        var key = urn[..(urn.Length - urnSuffix.Length - 1)];

        if (!ParameterValidation.RequestValidUrns.Contains(key))
        {
            return null;
        }

        return key switch
        {
            "urn:altinn:person:identifier-no" => await entityService.GetByPersNo(urnSuffix, ct),
            "urn:altinn:organization:identifier-no" => await entityService.GetByOrgNo(urnSuffix, ct),
            "urn:altinn:systemuser:uuid" or "urn:altinn:party:uuid" => await entityService.GetEntity(Guid.Parse(urnSuffix), ct),
            _ => null,
        };
    }
}
