using System.Net.Mime;
using Altinn.AccessManagement.Api.ServiceOwner.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
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
    IPackageService packageService) : ControllerBase
{
    /// <summary>
    /// Get valid urn prefixes for party identification
    /// </summary>
    [HttpGet("_meta/urns/party")]
    [FeatureGate(RequirementType.Any, AccessMgmtFeatureFlags.EnableRequestAssignmentResource, AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    public IActionResult GetValidUrns()
    {
        return Ok(RequestValidation.ValidUrns);
    }

    /// <summary>
    /// Get resource requests for a given party
    /// </summary>
    [HttpGet("{id}")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequest([FromQuery] string from, [FromQuery] string to, [FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await requestService.GetRequest(id, ct);

        if (result is { })
        {
            return Ok(result);
        }

        return NotFound();
    }

    /// <summary>
    /// Get resource requests for a given party
    /// </summary>
    [HttpGet("{id}/status")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestStatus>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRequestStatus([FromQuery] string from, [FromQuery] string to, [FromRoute] Guid id, CancellationToken ct = default)
    {
        var result = await requestService.GetRequest(id, ct);

        if (result is { })
        {
            return Ok(result.Status);
        }

        return NotFound();
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateServiceOwnerRequest input, CancellationToken ct = default)
    {
        var validationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestInput(input));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var from = await GetEntityByUrn(input.Connection.From, ct);
        var to = await GetEntityByUrn(input.Connection.To, ct);
        var role = RoleConstants.Rightholder;
        var status = RequestStatus.Draft;
        var resource = input.Resource is { } ? await resourceService.GetResource(input.Resource, ct) : null;
        var package = input.Package is { } ? await packageService.GetPackage(input.Package, ct) : null;

        var serviceValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestServiceInput(from, to, role, resource, package));
        if (serviceValidationErrors is { })
        {
            return serviceValidationErrors.ToActionResult();
        }

        var result = await requestService.CreateRequest(
            new CreateRequestDto()
            {
                From = from.Id,
                To = to.Id,
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

        result.Value.Links = BuildLinks(result.Value.Id);

        return Accepted(result.Value);
    }

    private static RequestLinks BuildLinks(Guid requestId) => new()
    {
        ConfirmLink = $"accessmanagement/api/v1/enduser/request/{requestId}/accept",
        StatusLink = $"accessmanagement/api/v1/serviceowner/delegationrequests/{requestId}"
    };

    private async Task<Entity> GetEntityByUrn(string urn, CancellationToken ct = default)
    {
        var urnSegments = urn.Split(":");
        var urnSuffix = urnSegments.Last();
        var key = urn[..(urn.Length - urnSuffix.Length - 1)];

        if (!RequestValidation.ValidUrns.Contains(key))
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
