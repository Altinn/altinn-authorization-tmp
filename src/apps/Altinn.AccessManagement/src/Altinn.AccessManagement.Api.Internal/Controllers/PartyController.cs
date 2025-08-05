using Altinn.AccessManagement.Api.Internal.Extensions;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.Authorization.Api.Contracts.Party;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESSTOKEN_ISSUER)]
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESSTOKEN_APP_AUTHENTICATION)]
    public class PartyController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IPartyService _partyService;

        public PartyController(
            ILogger<PartyController> logger,
            IPartyService partyService)
        {
            _logger = logger;
            _partyService = partyService;
        }

        [HttpPost]
        [Route("accessmanagement/api/v1/internal/party")]
        public async Task<ActionResult<AddPartyResultDto>> AddParty([FromBody] PartyBaseDto party, CancellationToken cancellationToken = default)
        {
            var options = new ChangeRequestOptions()
            {
                ChangedBy = party.CreatedBy ?? AuditDefaults.InternalApiImportSystem,
                ChangedBySystem = AuditDefaults.InternalApiImportSystem
            };

            var res = await _partyService.AddParty(party.ToCore(), options, cancellationToken);

            if (res.IsProblem)
            {
                return res.Problem.ToActionResult();
            }

            return Ok(res.Value.ToPartyResultDto());
        }
    }
}
