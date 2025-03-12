using System.Net.Mime;
using Altinn.AccessManagement.Api.Enterprise.Utils;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Enterprise.Controllers
{
    /// <summary>
    /// The default constructor taking in depencies. 
    /// </summary>
    [Route("accessmanagement/api/v1/enterprise/consent/")]
    [ApiController]
    public class ConsentController(IConsent consentService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;

        // Existing code...
        [HttpPost]
        [Route("request")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ConsentRequestStatusExternal), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(void), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateRequest([FromBody] ConsentRequestExternal consentRequest)
        {
            Result<ConsentRequestDetails> consentRequestStatus = await _consentService.CreateRequest(ModelMapper.ToCore(consentRequest));

            if (consentRequestStatus.IsProblem)
            {
                return consentRequestStatus.Problem.ToActionResult(); // This line will now work with the extension method
            }

            return Created($"/accessmanagement/api/v1/enterprice/concent/request/{consentRequestStatus.Value.Id}", consentRequestStatus.Value);
        }
    }
}
