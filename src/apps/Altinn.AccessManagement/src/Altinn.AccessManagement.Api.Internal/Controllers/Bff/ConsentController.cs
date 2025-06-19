using System.Security.Claims;
using Altinn.AccessManagement.Api.Internal.Extensions;
using Altinn.AccessManagement.Api.Internal.Utils;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ABAC.Xacml.JsonProfile;
using Altinn.Authorization.Api.Contracts.Consent;
using Altinn.Authorization.ProblemDetails;
using Altinn.Common.PEP.Helpers;
using Altinn.Common.PEP.Interfaces;
using Azure.Core;
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
    public class ConsentController(IConsent consentService, IPDP pdp) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;
        private readonly IPDP _pdp = pdp;

        /// <summary>
        /// Get a specific consent. 
        /// Requires the following.
        /// User is authenticated
        /// User has write access to access management for the party that is requested to consent (from party)
        /// User is authorized to delegate the rights that are requested. Either by having the right themselves or being the main administrator
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
        [Route("consentrequests/{requestId}", Name ="bffgetconsentrequest")]
        public async Task<IActionResult> GetConsentRequest([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Core.Models.Consent.ConsentPartyUrn performedByParty = Core.Models.Consent.ConsentPartyUrn.PartyUuid.Create(performedBy.Value);

            Result<ConsentRequestDetails> consentRequest = await _consentService.GetRequest(requestId, performedByParty, true, cancellationToken);
            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            // Check if the user is authorized to view the consent request. Anyone with read access to access management can view the consent request details.
            if (consentRequest.Value.From.IsPartyUuid(out Guid resourePartUuuid))
            {
                bool isAuthorized = await AuthorizeResourceAccess("altinn_access_management", resourePartUuuid, User, "read");

                if (isAuthorized)
                {
                    return Ok(consentRequest.Value.ToConsentRequestDetailsBFF());
                }
            }

            return Unauthorized();
        }

        /// <summary>
        /// Get a specific consent. 
        /// Requires the following.
        /// User is authenticated
        /// User is have write access to access management for the party that is requestesd to consent (from party)
        /// User is authorized to delegated the rights that is requested. Either by having the right self or beeing the main administrator
        /// </summary>
        [HttpGet]
        [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
        [Route("consents/{consentId}/")]
        public async Task<IActionResult> GetConsent([FromRoute] Guid consentId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<Consent> consent = await consentService.GetConsent(consentId, cancellationToken);
            if (consent.IsProblem)
            {
                consent.Problem.ToActionResult();
            }

            // Check if the user is authorized to view the consent request. Anyone with read access to access management can view the consent request details.
            if (consent.Value != null && consent.Value.From.IsPartyUuid(out Guid resourePartUuuid))
            {
                bool isAuthorized = await AuthorizeResourceAccess("altinn_access_management", resourePartUuuid, User, "read");

                if (isAuthorized)
                {
                    return Ok(consent.Value);
                }
            }

            return Unauthorized();
        }

        /// <summary>
        /// Endpoint to get the redirect URL for a consent request.
        /// </summary>
        [HttpGet]
        [Route("consentrequests/{requestId}/redirecturl")]
        public async Task<IActionResult> GetConsentRedirectUrl([FromRoute] Guid requestId, CancellationToken cancellationToken = default)
        {
            string redirectUrl = await consentService.GetRequestRedirectUrl(requestId, cancellationToken);
            if (string.IsNullOrEmpty(redirectUrl))
            {
                return NotFound("Redirect URL not found for the consent request.");
            }

            return Ok(new ConsentRedirectUrlDto() { Url = redirectUrl });
        }

        /// <summary>
        /// Endpoint to approve a consent request
        /// The authenticated user must fullfill the requirements to approve the request.
        /// - Have right for accessmanagement for the party that is requesting the consent
        /// - Have the right to delegate the requested rights. Either by having the right self or beeing the main administrator
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
        [Route("consentrequests/{requestId}/accept")]
        public async Task<IActionResult> Accept(Guid requestId, [FromBody] ConsentContextDto context, CancellationToken cancellationToken = default)
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

            Core.Models.Consent.ConsentPartyUrn performedByParty = Core.Models.Consent.ConsentPartyUrn.PartyUuid.Create(performedBy.Value);
            Result<ConsentRequestDetails> consentRequest = await _consentService.GetRequest(requestId, performedByParty, true, cancellationToken);
            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            // Check if the user is authorized to accept consent request in general. 
            if (consentRequest.Value.From.IsPartyUuid(out Guid resourePartUuuid))
            {
                bool isAuthorized = await AuthorizeResourceAccess("altinn_access_management", resourePartUuuid, User, "write");

                if (!isAuthorized)
                {
                    return Unauthorized();
                }
            }

            ConsentContext consentContext = context.ToConsentContext();

            consentRequest = await _consentService.AcceptRequest(requestId, performedBy.Value, consentContext, cancellationToken);

            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value.ToConsentRequestDetailsBFF());
        }

        /// <summary>
        /// Endpoint to deny a consent request
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
        [Route("consentrequests/{requestId}/reject")]
        public async Task<IActionResult> Reject(Guid requestId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Core.Models.Consent.ConsentPartyUrn performedByParty = Core.Models.Consent.ConsentPartyUrn.PartyUuid.Create(performedBy.Value);
            Result<ConsentRequestDetails> consentRequest = await _consentService.GetRequest(requestId, performedByParty, true, cancellationToken);
            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            // Check if the user is authorized to reject consent request in general. 
            if (consentRequest.Value.From.IsPartyUuid(out Guid resourePartUuuid))
            {
                bool isAuthorized = await AuthorizeResourceAccess("altinn_access_management", resourePartUuuid, User, "write");

                if (!isAuthorized)
                {
                    return Unauthorized();
                }
            }

            consentRequest = await _consentService.RejectRequest(requestId, performedBy.Value, cancellationToken);

            if (consentRequest.IsProblem)
            {
                return consentRequest.Problem.ToActionResult();
            }

            return Ok(consentRequest.Value.ToConsentRequestDetailsBFF());
        }

        /// <summary>
        /// Endpoint to deny a consent request
        /// </summary>
        [HttpPost]
        [Authorize(Policy = AuthzConstants.SCOPE_PORTAL_ENDUSER)]
        [Route("consents/{consentId}/revoke")]
        public async Task<IActionResult> Revoke(Guid consentId, CancellationToken cancellationToken = default)
        {
            Guid? performedBy = UserUtil.GetUserUuid(User);
            if (performedBy == null)
            {
                return Unauthorized();
            }

            Result<Consent> consent = await consentService.GetConsent(consentId, cancellationToken);
            if (consent.IsProblem)
            {
                consent.Problem.ToActionResult();
            }

            // Check if the user is authorized to view the consent request. Anyone with read access to access management can view the consent request details.
            if (consent.Value != null && consent.Value.From.IsPartyUuid(out Guid resourePartUuuid))
            {
                bool isAuthorized = await AuthorizeResourceAccess("altinn_access_management", resourePartUuuid, User, "write");

                if (!isAuthorized)
                {
                    return Unauthorized();
                }
            }

            Result<ConsentRequestDetails> result = await _consentService.RevokeConsent(consentId, performedBy.Value, cancellationToken);

            if (result.IsProblem)
            {
                return result.Problem.ToActionResult();
            }

            return Ok(result.Value);
        }

        private async Task<bool> AuthorizeResourceAccess(string resource, Guid resourceParty, ClaimsPrincipal userPrincipal,  string action)
        {
            XacmlJsonRequestRoot request = DecisionHelper.CreateDecisionRequestForResourceRegistryResource(resource, resourceParty, userPrincipal, "read");
            XacmlJsonResponse response = await _pdp.GetDecisionForRequest(request);

            if (response?.Response == null)
            {
                throw new ArgumentNullException("response");
            }

            if (!DecisionHelper.ValidatePdpDecision(response.Response, userPrincipal))
            {
                return false;
            }

            return true;
        }
    }
}
