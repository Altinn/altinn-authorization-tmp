using Altinn.AccessManagement.Api.Enduser.Models;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Models;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using System.Net.Mime;

namespace Altinn.AccessManagement.Api.Enduser.Controllers
{
    /// <summary>
    /// Controller for en user api operations for connections
    /// </summary>
    [ApiController]
    [Route("accessmanagement/api/v1/enduser/clientadmin")]
    [FeatureGate(AccessManagementEnduserFeatureFlags.ControllerConnections)]
    [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
    public class ClintAdminController
    {
        /// <summary>
        /// Get connections between the authenticated user's selected party and the specified target party.
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
        [ProducesResponseType<PaginatedResult<CompactRelationDto>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetClientConnections([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add agent role connection to hold client pagages
        /// </summary>
        [HttpPost]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType<Assignment>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddAssignment([FromQuery] ConnectionInput connection, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove agent role connection and all connected packages
        /// </summary>
        [HttpDelete]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveAssignment([FromQuery] ConnectionInput connection, [FromQuery] bool cascade = false, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get packages connected to the agent assignment between the selected client party and the specified target party for the given service entity where the authenticated user has acces to the required scope.
        /// </summary>
        [HttpGet("accesspackages")]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [ProducesResponseType<PaginatedResult<PackagePermission>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPackages([FromQuery] ConnectionInput connection, [FromQuery, FromHeader] PagingInput paging, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add package to a agent assignment for the given service entity between a given client party and the specified target party.
        /// </summary>
        [HttpPost("accesspackages")]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType<AssignmentPackage>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove package to a agent assignment for the given service entity between a given client party and the specified target party.
        /// </summary>
        [HttpDelete("accesspackages")]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemovePackages([FromQuery] ConnectionInput connection, [FromQuery] Guid? packageId, [FromQuery] string package, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// API for delegation check of access packages, for which packages the authenticated user has permission to assign to others on behalf of the specified party.
        /// </summary>
        [HttpGet("accesspackages/delegationcheck")]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType<PaginatedResult<AccessPackageDto.Check>>(StatusCodes.Status200OK)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CheckPackage([FromQuery] Guid party, [FromQuery] IEnumerable<Guid>? packageIds, [FromQuery] IEnumerable<string>? packages, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Lookup point where it is posible to fetch an Entity With the identifiers based on a lookp identity
        /// Organisasjonsnummer, Fødselsnummer (med mer informasjon som etternavn)
        /// </summary>
        /// <param name="lookup"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A complete entity with uuid and other available information</returns>
        /// <exception cref="NotImplementedException"></exception>
        [HttpPost("entitylookup")]
        [DbAudit(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApiStr)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_READ)]
        public async Task<IActionResult> EntityLookup([FromBody] EntityLookup lookup, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
