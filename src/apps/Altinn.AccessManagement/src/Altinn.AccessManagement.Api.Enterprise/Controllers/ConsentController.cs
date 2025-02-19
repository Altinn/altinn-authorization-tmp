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
 
    }
}
