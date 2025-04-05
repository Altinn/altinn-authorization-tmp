using Altinn.AccessManagement.Api.Enduser.Utils;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enduser.Controllers
{
    /// <summary>
    /// Api for consent information for end users.
    /// Most API is are only available from Altinn Portal. This to ensure that end user is web informed about details 
    /// </summary>
    [Route("accessmanagement/api/v1/enduser/consent/")]
    [ApiController]
    public class ConsentController(IConsent consentService, IPartiesClient partiesClient, ISingleRightsService singleRightsService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;
        private readonly IPartiesClient _partiesClient = partiesClient;

        /// <summary>
        /// Get a specific consent. 
        /// Requires the following.
        /// User is authenticated
        /// User is have write access to access management for the party that is requestesd to consent (from party)
        /// User is authorized to delegated the rights that is requested. Either by having the right self or beeing the main administrator
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("request/{requestId}")]
        public async Task<IActionResult> GetConsentRequest([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            ConsentRequestDetails consentRequest = await _consentService.GetRequest(requestId, performedBy.Value, cancellationToken);
            return Ok(consentRequest);
        }

        /// <summary>
        /// Endpoint to approve a consent request
        /// The authenticated user must fullfill the requirements to approve the request.
        /// - Have right for accessmanagement for the party that is requesting the consent
        /// - Have the right to delegate the requested rights. Either by having the right self or beeing the main administrator
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("request/{requestId}/accept/")]
        public async Task<IActionResult> Approve(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<ConsentRequestDetails> consentRequest = await _consentService.AcceptRequest(requestId, performedBy.Value, cancellationToken);

            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value);
        }

        /// <summary>
        /// Endpoint to deny a consent request
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("request/{requestId}/reject/")]
        public async Task<IActionResult> Reject(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<ConsentRequestDetails> consentRequest = await _consentService.RejectRequest(requestId, performedBy.Value, cancellationToken);

            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value);
        }

        /// <summary>
        /// Endpoint to deny a consent request
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("request/{requestId}/revoke/")]
        public async Task<IActionResult> Revoke(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<ConsentRequestDetails> result = await _consentService.RevokeConsent(requestId, performedBy.Value, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }
    }
}
