using System.Net.Mime;
using Altinn.AccessManagement.Api.Enterprise.Utils;
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
    [Route("accessmanagement/api/v1/enterprise/consent")]
    [ApiController]
    public class ConsentController(IConsent consentService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;

        /// <summary>
        /// Endpoint to create a consent request for
        /// </summary>
        [Authorize]
        [HttpPost]
        [Route("request", Name ="enterprisecreaterequest")]
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

            if (consentPartyUrn == null)
            {
                return Unauthorized();
            }

            Result<ConsentRequestDetails> consentRequestStatus = await _consentService.CreateRequest(ModelMapper.ToCore(consentRequest), consentPartyUrn, cancellationToken);

            if (consentRequestStatus.IsProblem)
            {
                return consentRequestStatus.Problem.ToActionResult();
            }

            if (consentRequestStatus.Value == null)
            {
                return BadRequest("Consent request could not be created");
            }

            var routeValues = new { consentRequestId = consentRequestStatus.Value.Id };
            string? locationUrl = Url.Link("enterprisegetrequest", routeValues);

            return Created(locationUrl, consentRequestStatus.Value);
        }

        [Authorize]
        [HttpGet]
        [Route("request/{consentRequestId:guid}", Name="enterprisegetrequest")]
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
