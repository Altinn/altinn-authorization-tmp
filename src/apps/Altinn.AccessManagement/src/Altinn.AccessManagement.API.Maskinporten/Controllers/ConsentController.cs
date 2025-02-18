using Altinn.AccessManagement.Api.Maskinporten.Models.Concent;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Register.Core.Parties;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Maskinporten.Controllers
{
    /// <summary>
    /// Comcent controller for Maskinporten
    /// </summary>
    [Route("accessmanagment/api/v1/maskinporten/")]
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
        /// Returns a specific consent
        /// </summary>
        [HttpGet]
        [Route("consent/lookup")]
        public async Task<ActionResult<ConsentInfoMaskinporten>> GetConcent(Guid id, string from, string to)
        {
            Consent consent = await _consentService.GetConcent(id, from, to);

            if (consent == null)
            {
                return NotFound();
            }

            return Ok(ConsentInfoMaskinporten.Convert(consent));
        }
    }
}
