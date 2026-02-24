using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Design;
using System.Net.Mime;

namespace Altinn.AccessManagement.Api.ServiceOwner.Controllers
{
    [ApiController]
    [Route("accessmanagement/api/v1/serviceowner/connections")]
    [Authorize(Policy = AuthzConstants.SCOPE_SERVICEOWNER_PACKAGE_WRITE)]
    public class ConnectionsController(
    IConnectionServiceServiceOwner connectionService,
    IUserProfileLookupService UserProfileLookupService,
    IEntityService EntityService,
    IResourceService resourceService
    ) : ControllerBase
    {

        [HttpPost("accesspackages")]
        [AuditJWTClaimToDb(Claim = AltinnCoreClaimTypes.PartyUuid, System = AuditDefaults.EnduserApi)]
        [Authorize(Policy = AuthzConstants.POLICY_ACCESS_MANAGEMENT_ENDUSER_WRITE)]
        [ProducesResponseType<AssignmentPackageDto>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
        [ProducesResponseType<AltinnProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddPackages([FromBody] ServiceOwnerAccessPackageDelegation packageDelegation, CancellationToken cancellationToken = default)
        {
            
            if(packageDelegation.From.)

            EntityService.GetByPersNo
        }
    }
}
