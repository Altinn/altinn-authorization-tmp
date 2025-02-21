using System.Net.Mime;
using Altinn.AccessManagement.Api.Enterprise.Models.Consent;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enterprise.Controllers
{

    [Route("accessmanagment/api/v1/enterprise/consent/")]
    [ApiController]
    public class ConsentController : ControllerBase
    {
        private readonly IConsent _consentService;

        /// <summary>
        /// The default constructor taking in depencies. 
        /// </summary>
        public ConsentController(IConsent consentService)
        {
            _consentService = consentService;
        }

        // Existing code...
        [HttpPost]
        [Route("request")]        [Consumes(MediaTypeNames.Application.Json)]        [Produces(MediaTypeNames.Application.Json)]        [ProducesResponseType(typeof(ConsentRequestStatusExternal), StatusCodes.Status200OK)]        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]        public async Task<ActionResult> CreateRequest([FromBody] ConsentRequestExternal consentRequest)
        {
            Result<ConsentRequestDetails> consentRequestStatus = await _consentService.CreateRequest(consentRequest.ToCore());

            if (consentRequestStatus.IsProblem)
            {
                return consentRequestStatus.Problem.ToActionResult(); // This line will now work with the extension method
            }

            return Created($"/accessmanagment/api/v1/enterprice/concent/request/{consentRequestStatus.Value.Id}", consentRequestStatus.Value);
        }
    }
}
