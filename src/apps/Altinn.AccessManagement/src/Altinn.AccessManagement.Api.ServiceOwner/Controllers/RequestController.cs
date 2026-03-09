using System.Net.Mime;
using Altinn.AccessManagement.Api.ServiceOwner.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
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
//// [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_SERVICEOWNER)]
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
        return Ok(Validation.RequestValidation.ValidUrns);
    }

    /// <summary>
    /// Get resource requests for a given party
    /// </summary>
    [HttpGet("resource")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<IEnumerable<RequestResourceDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindResourceRequests([FromQuery] RequestServiceOwnerQuery input, CancellationToken ct = default)
    {
        var validationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestInput(input));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var from = await GetEntityByUrn(input.From, ct);
        var to = await GetEntityByUrn(input.To, ct);

        var serviceValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestServiceInput(from, to));
        if (serviceValidationErrors is { })
        {
            return serviceValidationErrors.ToActionResult();
        }

        var requests = await requestService.GetRequestAssignmentResource(
            fromId: from.Id,
            toId: to.Id,
            roleId: null,
            resourceId: null,
            status: [RequestStatus.None, RequestStatus.Pending, RequestStatus.Approved],
            after: null,
            ct: ct);

        return Ok(requests.Select(r => DtoMapper.Convert(r)));
    }

    /// <summary>
    /// Get package requests for a given party
    /// </summary>
    [HttpGet("package")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<IEnumerable<RequestPackageDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindPackageRequests([FromQuery] RequestServiceOwnerQuery input, CancellationToken ct = default)
    {
        var validationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestInput(input));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var from = await GetEntityByUrn(input.From, ct);
        var to = await GetEntityByUrn(input.To, ct);

        var serviceValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestServiceInput(from, to));
        if (serviceValidationErrors is { })
        {
            return serviceValidationErrors.ToActionResult();
        }

        var requests = await requestService.GetRequestAssignmentPackage(
            fromId: from.Id,
            toId: to.Id,
            roleId: null,
            packageId: null,
            status: [RequestStatus.None, RequestStatus.Pending, RequestStatus.Approved],
            after: null,
            ct: ct);

        return Ok(requests.Select(r => DtoMapper.Convert(r)));
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost("resource")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentResource)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestResource([FromBody] CreateResourceRequestInput input, CancellationToken ct = default)
    {
        var validationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestResource(input));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var from = await GetEntityByUrn(input.Connection.From, ct);
        var to = await GetEntityByUrn(input.Connection.To, ct);
        var role = RoleConstants.Rightholder;
        var resource = await resourceService.GetResource(input.Resource.ResourceId, ct);

        var serviceValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestServiceInput(from, to, role, resource));
        if (serviceValidationErrors is { })
        {
            return serviceValidationErrors.ToActionResult();
        }

        var request = await requestService.CreateRequestAssignmentResource(from.Id, to.Id, role.Id, resource.Id, ct: ct);
        var result = ConvertResource(request);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Accepted(result.Value);
    }

    /// <summary>
    /// Create a package request for a given party and access package
    /// </summary>
    [HttpPost("package")]
    [FeatureGate(AccessMgmtFeatureFlags.EnableRequestAssignmentPackage)]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestPackageDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestPackage([FromBody] CreatePackageRequestInput input, CancellationToken ct = default)
    {
        var validationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestPackage(input));
        if (validationErrors is { })
        {
            return validationErrors.ToActionResult();
        }

        var from = await GetEntityByUrn(input.Connection.From, ct);
        var to = await GetEntityByUrn(input.Connection.To, ct);
        var role = RoleConstants.Rightholder;
        var package = await packageService.GetPackageByUrnValue(input.Package.Urn, ct);

        var serviceValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestServiceInput(from, to, role, package));
        if (serviceValidationErrors is { })
        {
            return serviceValidationErrors.ToActionResult();
        }

        var request = await requestService.CreateRequestAssignmentPackage(from.Id, to.Id, role.Id, package.Id, ct: ct);
        var result = ConvertPackage(request);

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Accepted(result.Value);
    }

    private Result<RequestResourceDto> ConvertResource(RequestAssignmentResource request)
    {
        var dtoValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestResourceDto(request));
        if (dtoValidationErrors is { })
        {
            return dtoValidationErrors;
        }

        return new RequestResourceDto
        {
            Id = request.Id,
            RequestType = "resource",
            Resource = new ResourceReferenceDto { ResourceId = request.Resource.RefId },
            Status = request.Status,
            Links = BuildLinks(request.Id),
            Connection = new ConnectionRequestDto
            {
                From = DtoMapper.ConvertToPartyEntityDto(request.Assignment.From),
                To = DtoMapper.ConvertToPartyEntityDto(request.Assignment.To),
            }
        };
    }

    private Result<RequestPackageDto> ConvertPackage(RequestAssignmentPackage request)
    {
        var dtoValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestPackageDto(request));
        if (dtoValidationErrors is { })
        {
            return dtoValidationErrors;
        }

        return new RequestPackageDto
        {
            Id = request.Id,
            RequestType = "package",
            Package = new PackageReferenceDto { Urn = request.Package.Urn },
            Status = request.Status,
            Links = BuildLinks(request.Id),
            Connection = new ConnectionRequestDto
            {
                From = DtoMapper.ConvertToPartyEntityDto(request.Assignment.From),
                To = DtoMapper.ConvertToPartyEntityDto(request.Assignment.To),
            }
        };
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
