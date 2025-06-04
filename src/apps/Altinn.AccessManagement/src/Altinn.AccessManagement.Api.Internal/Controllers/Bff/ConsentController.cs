using Altinn.AccessManagement.Api.Internal.Utils;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers.Bff
{
    /// <summary>
    /// API controller for managing consent information for end users.
    /// All endpoints are accessible only from the Altinn Portal to ensure that end users are properly informed about the details of their consents.
    /// The controller enforces the portal scope for authorization to access its methods.
    /// </summary>
    [Route("accessmanagement/api/v1/bff")]
    [ApiController]
    [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
    public class ConsentController(IConsent consentService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;

        /// <summary>
        /// Get a specific consent. 
        /// Requires the following.
        /// User is authenticated
        /// User has write access to access management for the party that is requested to consent (from party)
        /// User is authorized to delegate the rights that are requested. Either by having the right themselves or being the main administrator
        /// </summary>
        [HttpGet]
        [Route("consentrequests/{requestId}", Name ="bffgetconsentrequest")]
        public async Task<IActionResult> GetConsentRequest([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            ConsentPartyUrn performedByParty = ConsentPartyUrn.PartyUuid.Create(performedBy.Value);

            Result<ConsentRequestDetails> consentRequest = await _consentService.GetRequest(requestId, performedByParty, false, cancellationToken);
            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value);
        }

        /// <summary>
        /// Get a specific consent. 
        /// Requires the following.
        /// User is authenticated
        /// User is have write access to access management for the party that is requestesd to consent (from party)
        /// User is authorized to delegated the rights that is requested. Either by having the right self or beeing the main administrator
        /// </summary>
        [HttpGet]
        [Route("consents/{requestId}/")]
        public async Task<IActionResult> GetConsent([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<Consent> consent = await consentService.GetConsent(requestId, cancellationToken);
            if (consent.IsProblem)
            {
                consent.Problem.ToActionResult();
            }

            return Ok(consent.Value);
        }

        /// <summary>
        /// Endpoint to approve a consent request
        /// The authenticated user must fullfill the requirements to approve the request.
        /// - Have right for accessmanagement for the party that is requesting the consent
        /// - Have the right to delegate the requested rights. Either by having the right self or beeing the main administrator
        /// </summary>
        [HttpPost]
        [Route("consentrequests/{requestId}/accept")]
        public async Task<IActionResult> Accept(Guid requestId, [FromBody] ConsentContextExternal context, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            if (context == null)
            {
                return BadRequest("Consent context is required");
            }

            ConsentContext consentContext = context.ToCore();

            Result<ConsentRequestDetails> consentRequest = await _consentService.AcceptRequest(requestId, performedBy.Value, consentContext, cancellationToken);

            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value);
        }

        /// <summary>
        /// Endpoint to deny a consent request
        /// </summary>
        [HttpPost]
        [Route("consentrequests/{requestId}/reject")]
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
        [HttpPost]
        [Route("consents/{consentId}/revoke")]
        public async Task<IActionResult> Revoke(Guid consentId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<ConsentRequestDetails> result = await _consentService.RevokeConsent(consentId, performedBy.Value, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }
    }
}
