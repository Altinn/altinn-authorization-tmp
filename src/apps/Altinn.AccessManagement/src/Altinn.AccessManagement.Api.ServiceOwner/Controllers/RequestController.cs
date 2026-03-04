using Altinn.AccessManagement.Api.ServiceOwner.Validation;
using Altinn.AccessManagement.Core.Constants;
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
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers;

/// <summary>
/// Request access
/// </summary>
[ApiController]
[Route("accessmanagement/api/v1/serviceowner/delegationrequests")]
public class RequestController(IRequestService requestService, IEntityService entityService, IResourceService resourceService) : ControllerBase
{
    /// <summary>
    /// Get valid urn prefixes for party identification
    /// </summary>
    [HttpGet("_meta/urns/party")]
    [ProducesResponseType(typeof(IReadOnlyCollection<RequestStatus>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValidUrns(CancellationToken cancellationToken = default)
    {
        return Ok(Validation.RequestValidation.ValidUrns);
    }  

    /// <summary>
    /// Get resourc requests for a given party
    /// </summary>
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_READ)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> FindResourceRequests([FromQuery] RequestQueryInput input, CancellationToken ct = default)
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
            return validationErrors.ToActionResult();
        }

        var requests = await requestService.GetRequests(fromId: from.Id, toId: to.Id, status: [RequestStatus.None, RequestStatus.Approved, RequestStatus.Pending], after: DateTimeOffset.UtcNow, ct: ct);
        return Ok();
    }

    /// <summary>
    /// Create a resource request for a given party and resource
    /// </summary>
    [HttpPost("resource")]
    [Authorize(Policy = AuthzConstants.ALTINN_SERVICEOWNER_DELEGATIONREQUESTS_WRITE)]
    [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.ServiceOwnerApi)]
    [ProducesResponseType<RequestResourceDto>(StatusCodes.Status202Accepted, MediaTypeNames.Application.Json)]
    [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RequestResource([FromBody] RequestResourceInput input, CancellationToken ct = default)
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
            return validationErrors.ToActionResult();
        }

        var request = await requestService.CreateRequestAssignmentResource(from.Id, to.Id, role.Id, resource.Id, ct);
        var result = Convert(request, "at22", "api/request");

        if (result.IsProblem)
        {
            return result.Problem.ToActionResult();
        }

        return Accepted(result.Value);
    }

    private Result<RequestResourceDto> Convert(RequestAssignmentResource request, string enduserLinkPrefix, string statusLinkPrefix)
    {
        var dtoValidationErrors = ValidationComposer.Validate(RequestValidation.ValidateRequestResourceDto(request));
        if (dtoValidationErrors is { })
        {
            return dtoValidationErrors;
        }

        return new RequestResourceDto()
        {
            Id = request.Id,
            Resource = new ResourceRefrenceDto() { ResourceId = request.Resource.RefId },
            Status = request.Status,
            Links = new RequestLinks()
            {
                EnduserLink = $"{enduserLinkPrefix}/{request.Id}",
                StatusLink = $"{statusLinkPrefix}/{request.Id}"
            },
            Connection = new ConnectionRequestDto()
            {
                From = DtoMapper.ConvertToPartyEntityDto(request.Assignment.From),
                To = DtoMapper.ConvertToPartyEntityDto(request.Assignment.To),
            }
        };
    }

    private async Task<Entity> GetEntityByUrn(string urn, CancellationToken ct = default)
    {
        var s = urn.Split(":");
        var value = s.Last();
        var key = urn.Substring(0, urn.Length - value.Length + 1);

        if (!RequestValidation.ValidUrns.Contains(key))
        {
            return null;
        }

        switch (key)
        {
            case "urn:altinn:person:identifier-no":
                return await entityService.GetByPersNo(value, ct);
            case "urn:altinn:organization:identifier-no":
                return await entityService.GetByOrgNo(value, ct);
            case "urn:altinn:systemuser:uuid":
            case "urn:altinn:party:uuid":
                return await entityService.GetEntity(Guid.Parse(value), ct);
            default:
                return null;
        }
    }
}
