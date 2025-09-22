using System.IdentityModel.Tokens.Jwt;
using Altinn.AccessManagement.Api.Internal.Extensions;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.Authorization.Api.Contracts.Party;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.Controllers
{
    [Authorize(Policy = AuthzConstants.PLATFORM_ACCESSTOKEN_ISSUER_ISPLATFORM)]
    [Route("accessmanagement/api/v1/internal/party")]
    public class PartyController : ControllerBase
    {
        private readonly Altinn.AccessMgmt.Core.Services.Contracts.IPartyService corePartyService;
        private readonly Altinn.AccessMgmt.Persistence.Services.Contracts.IPartyService persistencePartyService;
        private readonly IFeatureManager featureManager;

        public PartyController(
            Altinn.AccessMgmt.Core.Services.Contracts.IPartyService corePartyService,
            Altinn.AccessMgmt.Persistence.Services.Contracts.IPartyService persistencePartyService,
            IFeatureManager featureManager)
        {
            this.corePartyService = corePartyService;
            this.persistencePartyService = persistencePartyService;
            this.featureManager = featureManager;
        }

        [HttpPost]
        public async Task<ActionResult<AddPartyResultDto>> AddParty([FromBody] PartyBaseDto party, [FromHeader(Name = "PlatformAccessToken")] string token, CancellationToken cancellationToken = default)
        {
            if (!CheckValidAppClaim(token))
            {
                return Unauthorized("Invalid app claim in platform access token.");
            }

            var options = new ChangeRequestOptions()
            {
                ChangedBy = party.CreatedBy ?? AuditDefaults.InternalApiImportSystem,
                ChangedBySystem = AuditDefaults.InternalApiImportSystem
            };

            Result<AddPartyResultDto> resDto = null;
            Result<AddPartyResult> res = null;

            if (await featureManager.IsEnabledAsync("AccessMgmt.PartyService.EFCore"))
            {
                // EFCore implementation
                resDto = await corePartyService.AddParty(party.ToCore(), cancellationToken);
            }
            else
            {
                // Persistence implementation
                res = await persistencePartyService.AddParty(party.ToCore(), options, cancellationToken);
            }

            if (resDto != null)
            {
                if (resDto.IsProblem)
                {
                    return resDto.Problem.ToActionResult();
                }
                var partyResultDto = resDto.Value;
                return partyResultDto.PartyCreated ? CreatedAtAction(nameof(AddParty), new { id = partyResultDto.PartyUuid }, partyResultDto) : Ok(partyResultDto);
            }
            else if (res != null)
            {
                if (res.IsProblem)
                {
                    return res.Problem.ToActionResult();
                }
                var partyResultDto = new AddPartyResultDto
                {
                    PartyUuid = res.Value.PartyUuid,
                    PartyCreated = res.Value.PartyCreated
                };
                return partyResultDto.PartyCreated ? CreatedAtAction(nameof(AddParty), new { id = partyResultDto.PartyUuid }, partyResultDto) : Ok(partyResultDto);
            }

            return BadRequest();
        }

        /// <summary>
        /// Validate app-claim from the platform token
        /// </summary>
        private static bool CheckValidAppClaim(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtSecurityToken = handler.ReadJwtToken(token);
                var appidentifier = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == AltinnXacmlConstants.MatchAttributeIdentifiers.AppAttribute);
                if (appidentifier != null)
                {
                    return appidentifier.Value.Equals("authentication", StringComparison.OrdinalIgnoreCase);
                }
            }

            return false;
        }
    }
}
