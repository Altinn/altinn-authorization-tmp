using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.Register;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers
{
    /// <summary>
    /// Connection service for service owner
    /// </summary>
    [ApiController]
    [Route("accessmanagement/api/v1/serviceowner/connections")]
    [Authorize(Policy = AuthzConstants.SCOPE_SERVICEOWNER_PACKAGE_WRITE)]
    public class ConnectionsController(
        IAssignmentService AssignmentService,
        IConnectionServiceServiceOwner connectionService,
        IUserProfileLookupService UserProfileLookupService,
        IEntityService EntityService,
        IPackageService packageService
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

        [HttpPost("accesspackages")]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPackages([FromBody] ServiceOwnerAccessPackageDelegation packageDelegation, CancellationToken cancellationToken = default)
        {
            Guid? fromPerson = null;
            Guid? toPerson = null;

            if (packageDelegation.From.IsPersonId(out PersonIdentifier person))
            {
                Entity personFot = await EntityService.GetByPersNo(person.ToString(), cancellationToken);
                fromPerson = personFot.Id;
            }

            if (packageDelegation.To.IsPersonId(out PersonIdentifier personTo))
            {
                Entity personFot = await EntityService.GetByPersNo(personTo.ToString(), cancellationToken);
                toPerson = personFot.Id;
            }

            if (fromPerson == null || toPerson == null)
            {
                return BadRequest("TODO");
            }

            PackageDto package = await packageService.GetPackageByUrnValue(packageDelegation.PackageUrn.ValueSpan.ToString(), cancellationToken);

            if (package is null)
            {
                return Problems.PackageNotFound.ToActionResult();
            }

            Result<AssignmentPackageDto> result = await connectionService.AddPackage(fromPerson.Value, toPerson.Value, package.Id, ConfigureConnections, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }
    }
}
