using Altinn.AccessManagement.Api.Enterprise.Models.Consent;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Endpoint for enterprise to create a consent request
        /// </summary>
        [HttpPost("request/", Name = "CreateRequest")]
        public async Task<ActionResult<ConsentRequestStatusExternal>> CreateRequest([FromBody] ConsentRequestExternal consentRequest)
        {
            ConsentRequestDetails consentRequestStatus = await _consentService.CreateRequest(consentRequest.ToCore());

            return Created($"/accessmanagment/api/v1/enerprice/concent/request/{consentRequestStatus.Id}", consentRequestStatus);
        }
    }
}
