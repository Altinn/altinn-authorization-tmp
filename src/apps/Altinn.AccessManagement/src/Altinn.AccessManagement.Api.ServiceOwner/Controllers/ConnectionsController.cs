using System.Net.Mime;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Audit;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
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
    public class ConnectionsController(
        IConnectionServiceServiceOwner connectionService,
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
        [AuditServiceOwnerConsumer]
        [Authorize(Policy = AuthzConstants.SCOPE_SERVICEOWNER_PACKAGE_WRITE)]
        [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPackages([FromBody] ServiceOwnerAccessPackageDelegation packageDelegation, CancellationToken cancellationToken = default)
        {
            Guid? fromEntity = null;
            Guid? toEntity = null;

            if (packageDelegation.From.IsPersonId(out PersonIdentifier person))
            {
                Entity personFrom = await EntityService.GetByPersNo(person.ToString(), cancellationToken);
                fromEntity = personFrom?.Id;
            }

            if (packageDelegation.To.IsPersonId(out PersonIdentifier personTo))
            {
                Entity personToEntity = await EntityService.GetByPersNo(personTo.ToString(), cancellationToken);
                toEntity = personToEntity?.Id;
            }

             // Validate entities exist
            if (fromEntity is null || toEntity is null)
            {
                return Problems.ConnectionEntitiesDoNotExist.ToActionResult();
            }

            PackageDto package = await packageService.GetPackageByUrnValue(packageDelegation.PackageUrn.ValueSpan.ToString(), cancellationToken);

            if (package is null)
            {
                return Problems.PackageNotFound.ToActionResult();
            }

            Result<AssignmentPackageDto> result = await connectionService.AddPackage(fromEntity.Value, toEntity.Value, package.Id, ConfigureConnections, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }
    }
}
