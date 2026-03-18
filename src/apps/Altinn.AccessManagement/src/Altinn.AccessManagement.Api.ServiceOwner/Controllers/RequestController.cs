using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Altinn.AccessManagement.Api.ServiceOwner.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Audit;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement.Request;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers;

/// <summary>
/// Request access
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/serviceowner/delegationrequests")]
public class RequestController(
    IRequestService requestService,
    IEntityService entityService,
    IResourceService resourceService,
    IPackageService packageService,
    IAuditAccessor auditAccessor
    ) : ControllerBase
{
    /// <summary>
    /// Get valid urn prefixes for party identification
    /// </summary>
    [HttpGet("_meta/urns/party")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    public IActionResult GetValidUrns()
    {
        return Ok(ValidUrns);
    }

    /// <summary>
    /// Get resource requests for a given party
    /// </summary>
    [HttpGet("{id}/status")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditServiceOwnerConsumer]
    [ProducesResponseType<RequestStatus>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequestStatus([FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await requestService.GetRequest(id, ct);
        return result.IsSuccess ? Ok(result.Value.Status) : result.Problem.ToActionResult();
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost("resource")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditServiceOwnerConsumer]
    [ProducesResponseType<RequestDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateResourceRequest(
        [FromQuery][Required] string from,
        [FromQuery][Required] string to,
        [FromQuery][Required] string resource,
        [FromBody] string[]? rightKeys,
        CancellationToken ct = default
        )
    {
        var fromResult = await GetEntity(from, "$QUERY/from", ct);
        if (fromResult.IsProblem)
        {
            return fromResult.Problem.ToActionResult();
        }

        var toResult = await GetEntity(to, "$QUERY/to", ct);
        if (toResult.IsProblem)
        {
            return toResult.Problem.ToActionResult();
        }

        /*
        NAV (authUserUuid) ber om tilgang for Kari (party) til App (resource) hos Org (to).
        ==
        NAV (by) ber om tilgang for Kari (for) til App (resource) hos Org (at).
        */
        return await CreateResourceRequest(
            atId: fromResult.Value.Id,
            forId: toResult.Value.Id,
            byId: auditAccessor.AuditValues.ChangedBy,
            roleId: RoleConstants.Rightholder.Id,
            status: RequestStatus.Draft,
            resourceRef: new RequestRefrenceDto() { ReferenceId = resource }
            );
    }

    /// <summary>
    /// Create a package request
    /// </summary>
    [HttpPost("package")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditServiceOwnerConsumer]
    [ProducesResponseType<RequestDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePackageRequest(
        [FromQuery][Required] string from,
        [FromQuery][Required] string to,
        [FromQuery][Required] string package,
        [FromBody] string[]? rightKeys,
        CancellationToken ct = default
        )
    {
        var fromResult = await GetEntity(from, "$QUERY/from", ct);
        if (fromResult.IsProblem)
        {
            return fromResult.Problem.ToActionResult();
        }

        var toResult = await GetEntity(to, "$QUERY/to", ct);
        if (toResult.IsProblem)
        {
            return toResult.Problem.ToActionResult();
        }

        /*
        NAV (authUserUuid) ber om tilgang for Kari (party) til App (resource) hos Org (to).
        ==
        NAV (by) ber om tilgang for Kari (for) til App (resource) hos Org (at).
        */
        return await CreatePackageRequest(
            atId: fromResult.Value.Id,
            forId: toResult.Value.Id,
            byId: auditAccessor.AuditValues.ChangedBy,
            roleId: RoleConstants.Rightholder.Id,
            status: RequestStatus.Draft,
            packageRef: new RequestRefrenceDto() { ReferenceId = package }
            );
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditServiceOwnerConsumer]
    [ProducesResponseType<RequestDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateServiceOwnerRequest input, CancellationToken ct = default)
    {
        ValidationErrorBuilder errorBuilder = default;

        if (input.Resource.HasValue() && input.Package.HasValue())
        {
            errorBuilder.Add(ValidationErrorDescriptors.RequestResourceOrPackage);
            return errorBuilder.TryToProblemDetails(out var errorProblemDetails) 
                ? BadRequest(errorProblemDetails) 
                : BadRequest();
        }

        var fromResult = await GetEntity(input.From, "BODY/connection.from", ct);
        if (fromResult.IsProblem)
        {
            return fromResult.Problem.ToActionResult();
        }

        var toResult = await GetEntity(input.To, "BODY/connection.to", ct);
        if (toResult.IsProblem) 
        { 
            return toResult.Problem.ToActionResult(); 
        }

        /*
        NAV (authUserUuid) ber om tilgang for Kari (party) til App (resource) hos Org (to).
        ==
        NAV (by) ber om tilgang for Kari (for) til App (resource) hos Org (at).
        */

        if (input.Resource is { } && input.Resource.HasValue())
        {
            return await CreateResourceRequest(
                atId: fromResult.Value.Id,
                forId: toResult.Value.Id,
                byId: auditAccessor.AuditValues.ChangedBy,
                roleId: RoleConstants.Rightholder.Id,
                status: RequestStatus.Draft,
                resourceRef: input.Resource
                );
        }

        if (input.Package is { } && input.Package.HasValue())
        {
            return await CreatePackageRequest(
                atId: fromResult.Value.Id,
                forId: toResult.Value.Id,
                byId: auditAccessor.AuditValues.ChangedBy,
                roleId: RoleConstants.Rightholder.Id,
                status: RequestStatus.Draft,
                packageRef: input.Package
                );
        }

        errorBuilder.Add(ValidationErrorDescriptors.RequestedResourceNotFound);
        errorBuilder.Add(ValidationErrorDescriptors.RequestedPackageNotFound);
        errorBuilder.TryToProblemDetails(out var problemDetails);

        return problemDetails is { } ? BadRequest(problemDetails) : BadRequest();
    }

    private async Task<IActionResult> CreateResourceRequest(Guid atId, Guid forId, Guid byId, Guid roleId, RequestStatus status, RequestRefrenceDto resourceRef, CancellationToken ct = default)
    {
        ValidationErrorBuilder errorBuilder = default;

        var resource = await resourceService.GetResource(resourceRef, ct);

        if (resource == null)
        {
            errorBuilder.Add(ValidationErrorDescriptors.RequestedResourceNotFound, $"BODY/resource", [new("resource", $"Urn {resourceRef.ReferenceId} is not valid")]);
        }

        if (errorBuilder.TryBuild(out var problem))
        {
            return problem.ToActionResult();
        }

        var result = await requestService.CreateResourceRequest(
            atId: atId,
            forId: forId,
            byId: byId,
            roleId: roleId,
            resourceId: resource.Id,
            status: status,
            ct: ct
        );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        result.Value.Links = BuildLinks(result.Value.Id);

        return Accepted(result.Value);
    }

    private async Task<IActionResult> CreatePackageRequest(Guid atId, Guid forId, Guid byId, Guid roleId, RequestStatus status, RequestRefrenceDto packageRef, CancellationToken ct = default)
    {
        ValidationErrorBuilder errorBuilder = default;

        var package = await packageService.GetPackage(packageRef, ct);

        if (package == null)
        {
            errorBuilder.Add(ValidationErrorDescriptors.RequestedPackageNotFound, $"BODY/package", [new("package", $"Urn {packageRef.ReferenceId} is not valid")]);
        }

        if (errorBuilder.TryBuild(out var problem))
        {
            return problem.ToActionResult();
        }

        var result = await requestService.CreatePackageRequest(
           atId: atId,
           forId: forId,
           byId: byId,
           roleId: roleId,
           packageId: package.Id,
           status: status,
           ct: ct
       );

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        result.Value.Links = BuildLinks(result.Value.Id);

        return Accepted(result.Value);
    }

    private async Task<Result<Entity>> GetEntity(string urn, string paramName, CancellationToken ct = default)
    {
        ValidationErrorBuilder errorBuilder = default;

        if (!ValidUrn(urn))
        {
            errorBuilder.Add(ValidationErrorDescriptors.InvalidUrn, paramName, [new(paramName, $"Urn {urn} is not valid")]);
        }

        if (errorBuilder.TryBuild(out var problem))
        {
            return problem;
        }

        var urnSegments = urn.Split(":");
        var urnSuffix = urnSegments.Last();
        var key = urn[..(urn.Length - urnSuffix.Length - 1)];

        if (!ValidUrns.Contains(key))
        {
            errorBuilder.Add(ValidationErrorDescriptors.InvalidUrn, $"$QUERY/{paramName}", [new(paramName, $"Urn {urn} is not valid")]);
        }

        var entity = key switch
        {
            "urn:altinn:person:identifier-no" => await entityService.GetByPersNo(urnSuffix, ct),
            "urn:altinn:organization:identifier-no" => await entityService.GetByOrgNo(urnSuffix, ct),
            "urn:altinn:systemuser:uuid" or "urn:altinn:party:uuid" => await entityService.GetEntity(Guid.Parse(urnSuffix), ct),
            _ => null,
        };

        if (entity == null)
        {
            errorBuilder.Add(ValidationErrorDescriptors.NotFound, $"$QUERY/{paramName}", [new(paramName, $"Entity not found with matcing urn '{urn}'")]);
        }

        if (errorBuilder.TryBuild(out var problem1))
        {
            return problem1;
        }

        return entity;
    }
   
    private static bool ValidUrn(string urn) => ValidUrns.Any(t => urn.StartsWith(t));

    private static string[] ValidUrns => ["urn:altinn:person:identifier-no", "urn:altinn:organization:identifier-no"];

    private static RequestLinks BuildLinks(Guid requestId) => new()
    {
        DetailsLink = $"accessmanagement/api/v1/enduser/request/{requestId}/accept",
        StatusLink = $"accessmanagement/api/v1/serviceowner/delegationrequests/{requestId}"
    };
}
