using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Api.Enduser.Validation;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Core.Audit;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace Altinn.AccessManagement.Api.Enduser.Controllers
{
    [ApiController]
    [Route("accessmanagement/api/v1/enduser/bankruptcyestate")]
    [Tags("Bankruptcy Delegation")]
    public class BanckruptcyDelegationController(
        IHttpContextAccessor httpContextAccessor,
        IInputValidation inputValidation,
        IBankruptcyDelegationService bankruptcyDelegationService,
        IConnectionService ConnectionService) : ControllerBase
    {
        private Action<ConnectionOptions> ConfigureConnections { get; } = options =>
        {
            options.AllowedWriteFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
            options.AllowedWriteToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person, EntityTypeConstants.SystemUser];
            options.AllowedReadFromEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person];
            options.AllowedReadToEntityTypes = [EntityTypeConstants.Organization, EntityTypeConstants.Person, EntityTypeConstants.SystemUser];
            options.FilterFromEntityTypes = [];
            options.FilterToEntityTypes = [];
        };

        [HttpGet("users")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_READ)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_READ)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBankruptcyEstateAssignmentsForUser(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "user")] Guid user,
            [FromQuery, FromHeader] PagingInput paging,
            CancellationToken cancellationToken = default)
        {
            var result = await bankruptcyDelegationService.GetBankruptcyEstateAssignmentsForUser(party, user, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(PaginatedResult.Create(result.Value, null));
        }

        [HttpPost("users")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_WRITE)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_WRITE)]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [ProducesResponseType<AssignmentDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> AddUserToBankruptcyEstate(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "user")] Guid user,
            [FromQuery(Name = "bankruptcyestate")][Required] Guid bankruptcyestate,
            [FromBody] PersonInput? person,
            CancellationToken cancellationToken = default)
        {
            // Check that party has the bankruptcyestate as an active bankruptcyestate
            var hasConnection = await bankruptcyDelegationService.CheckBankruptcyEstateConnection(party, bankruptcyestate, cancellationToken);

            if (!hasConnection)
            {
                return Forbid();
            }

            var entity = await inputValidation.SanitizeToInput(
            party,
            user,
            person,
            options =>
            {
                options.AllowedToEntityTypes = [EntityTypeConstants.Person, EntityTypeConstants.Organization];
            },
            cancellationToken);

            if (entity.IsProblem)
            {
                return entity.Problem.ToActionResult();
            }

            var result = await ConnectionService.AddRightholder(bankruptcyestate, entity.Value.Id, ConfigureConnections, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }

        [HttpPost("users/delete")]
        [HttpDelete("users")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_WRITE)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_WRITE)]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteUserFromBankruptcyEstate(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "user")][Required] Guid user,
            [FromQuery(Name = "bankruptcyestate")][Required] Guid bankruptcyestate,
            [FromQuery(Name = "cascade")] bool cascade = false,
            CancellationToken cancellationToken = default)
        {
            // Check that party has the bankruptcyestate as an active bankruptcyestate
            var hasConnection = await bankruptcyDelegationService.CheckBankruptcyEstateConnection(party, bankruptcyestate, cancellationToken);

            if (!hasConnection)
            {
                return Forbid();
            }

            var problem = await ConnectionService.RemoveAssignment(bankruptcyestate, user, cascade, ConfigureConnections, cancellationToken);
            if (problem is { })
            {
                return problem.ToActionResult();
            }

            return NoContent();
        }

        [HttpPost("users/packages")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_WRITE)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_WRITE)]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPackageToUserForBankruptcyEstate(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "user")][Required] Guid user,
            [FromQuery(Name = "estate")][Required] Guid bankruptcyestate,
            [FromBody] List<string> packages,
            CancellationToken cancellationToken = default)
        {
            // TODO: Fix return value when user is deleted no content exist to return
            
            // Check that party has the bankruptcyestate as an active bankruptcyestate
            var hasConnection = await bankruptcyDelegationService.CheckBankruptcyEstateConnection(party, bankruptcyestate, cancellationToken);

            if (!hasConnection)
            {
                return Forbid();
            }

            var result = await bankruptcyDelegationService.AddBankruptcyEstatePackagesToUser(party, user, bankruptcyestate, packages, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }

        [HttpDelete("users/packages")]
        [HttpPost("users/packages/delete")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_WRITE)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_WRITE)]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeletePackageFromUserForBankruptcyEstate(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "user")][Required] Guid user,
            [FromQuery(Name = "estate")][Required] Guid bankruptcyestate,
            [FromBody][Required] List<string> packages,
            CancellationToken cancellationToken = default)
        {
            // Check that party has the bankruptcyestate as an active bankruptcyestate
            var hasConnection = await bankruptcyDelegationService.CheckBankruptcyEstateConnection(party, bankruptcyestate, cancellationToken);

            if (!hasConnection)
            {
                return Forbid();
            }

            var result = await bankruptcyDelegationService.RevokeBankruptcyEstatePackagesFromUser(party, user, bankruptcyestate, packages, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(PaginatedResult.Create(result.Value, null));
        }

        [HttpGet("estates")]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_READ)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_READ)]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBankruptcyEstateAssignmentsForEstate(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery(Name = "estate")][Required] Guid bankruptcyestate,
            [FromQuery, FromHeader] PagingInput paging,
            CancellationToken cancellationToken = default)
        {
            // Check that party has the bankruptcyestate as an active bankruptcyestate
            var hasConnection = await bankruptcyDelegationService.CheckBankruptcyEstateConnection(party, bankruptcyestate, cancellationToken);

            if (!hasConnection)
            {
                return Forbid();
            }

            var result = await bankruptcyDelegationService.GetBankruptcyEstateAssignmentsForEstate(party, bankruptcyestate, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(PaginatedResult.Create(result.Value, null));
        }

        [HttpGet]
        [Authorize(Policy = AuthzConstants.SCOPE_ENDUSER_BANKRUPTCYDELEGATION_READ)]
        [Authorize(Policy = AuthzConstants.POLICY_BANKRUPTCYDELEGATION_READ)]
        [ProducesResponseType<PaginatedResult<ClientDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetBankruptcyEstateAssignmentsForParty(
            [FromQuery(Name = "party")][Required] Guid party,
            [FromQuery, FromHeader] PagingInput paging,
            CancellationToken cancellationToken = default)
        {
            var result = await bankruptcyDelegationService.GetBankruptcyEstateForParty(party, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(PaginatedResult.Create(result.Value, null));
        }

    }
}
