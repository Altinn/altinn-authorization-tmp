using System.Net.Mime;
using Altinn.AccessManagement.Api.Enterprise.Utils;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enterprise.Controllers
{
    /// <summary>
    /// The default constructor taking in depencies. 
    /// </summary>
    [Route("accessmanagement/api/v1/enterprise")]
    [ApiController]
    public class ConsentController(IConsent consentService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;

        private const string CreateRouteName = "enterprisecreaterequest";
        private const string GetRouteName = "enterprisegetrequest";

        /// <summary>
        /// Endpoint to create a consent request for
        /// </summary>
        [Authorize(Policy = AuthzConstants.POLICY_CONSENTREQUEST_WRITE)]
        [HttpPost]
        [Route("consentrequests", Name = CreateRouteName)]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConsentRequestStatusExternal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateRequest([FromBody] ConsentRequestExternal consentRequest, CancellationToken cancellationToken = default)
        {
            ConsentPartyUrn? consentPartyUrn = OrgUtil.GetAuthenticatedParty(User);
            ConsentPartyUrn? supplierUrn = OrgUtil.GetSupplierParty(User);

            if (consentPartyUrn == null)
            {
                return Unauthorized();
            }

            ConsentRequest consentRequestInternal = ModelMapper.ToCore(consentRequest);
            if (supplierUrn != null)
            {
                consentRequestInternal.HandledBy = supplierUrn;
            }

            if (consentRequestInternal.To != consentPartyUrn)
            {
                // This scenario is only allowed for orgs creating consents for their own resources. 
                // Used in EBEVIS where Digdir request consent 
                // TODO: Add scope validation
                consentRequestInternal.HandledBy = consentPartyUrn;
            }

            Result<ConsentRequestDetailsWrapper> consentRequestStatus = await _consentService.CreateRequest(consentRequestInternal, consentPartyUrn, cancellationToken);

            if (consentRequestStatus.IsProblem)
            {
                return consentRequestStatus.Problem.ToActionResult();
            }

            var routeValues = new { consentRequestId = consentRequestStatus.Value.ConsentRequest.Id };
            string? locationUrl = Url.Link(GetRouteName, routeValues);

            if (consentRequestStatus.Value.AlreadyExisted)
            {
                return Ok(consentRequestStatus.Value.ConsentRequest);
            }

            return Created(locationUrl, consentRequestStatus.Value.ConsentRequest);
        }

        /// <summary>
        /// Returns the consent request. Only returns request details for the authenticated party.
        /// </summary>
        [Authorize]
        [HttpGet]
        [Route("consentrequests/{consentRequestId:guid}", Name= GetRouteName)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConsentRequestStatusExternal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetRequest([FromRoute] Guid consentRequestId, CancellationToken cancellationToken = default)
        {
            ConsentPartyUrn? consentPartyUrn = OrgUtil.GetAuthenticatedParty(User);

            if (consentPartyUrn == null)
            {
                return Unauthorized();
            }
            
            Result<ConsentRequestDetails> consentRequestStatus = await _consentService.GetRequest(consentRequestId, consentPartyUrn, cancellationToken);

            if (consentRequestStatus.IsProblem)
            {
                return consentRequestStatus.Problem.ToActionResult();
            }

            return Ok(consentRequestStatus.Value);
        }
    }
}
