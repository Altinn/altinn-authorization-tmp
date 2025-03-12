using Altinn.AccessManagement.Api.Enduser.Utils;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers
{
    
    /// <summary>
    /// Api for consent information for end users.
    /// Most API is are only available from Altinn Portal. This to ensure that end user is web informed about details 
    /// </summary>
    [Route("accessmanagement/api/v1/enduser/consent/")]
    [ApiController]
    public class ConsentController(IConsent consentService, IPartiesClient partiesClient) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;
        private readonly IPartiesClient _partiesClient = partiesClient;

        /// <summary>
        /// Get a specific consent
        /// </summary>
        [Route("request/{requestId}")]
        public async Task<IActionResult> GetConsentRequest([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            ConsentRequestDetails consentRequest = await _consentService.GetRequest(requestId, cancellationToken);
            return Ok(consentRequest);
        }

        [HttpPost]
        [Route("request/{requestId}/approve/")]
        public async Task<IActionResult> Approve(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            await _consentService.ApproveRequest(requestId, performedBy.Value, cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Route("request/{requestId}/revoke/")]
        public async Task<IActionResult> Deny(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            await _consentService.DenyRequest(requestId, performedBy.Value, cancellationToken);
            return Ok();
        }
    }
}
