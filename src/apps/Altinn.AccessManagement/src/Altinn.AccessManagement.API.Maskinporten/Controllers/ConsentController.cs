using Altinn.AccessManagement.Api.Maskinporten.Models.Concent;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Maskinporten.Controllers
{
    /// <summary>
    /// Comcent controller for Maskinporten
    /// </summary>
    [Route("accessmanagement/api/v1/maskinporten/consent/")]
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
        [HttpPost]
        [Route("lookup")]
        public async Task<ActionResult<ConsentInfoMaskinporten>> GetConcent([FromBody] ConsentLookup consentLookup)
        {
            ConsentPartyUrn from = consentLookup.From switch
            {
                ConsentPartyUrnExternal.PersonId => ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse(consentLookup.From.ValueSpan)),
                ConsentPartyUrnExternal.OrganizationId => ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(consentLookup.From.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            Result<Consent> consent = await _consentService.GetConsent(consentLookup.Id, MapToCore(consentLookup.From), MapToCore(consentLookup.To));

            if (consent.IsProblem)
            {
                return consent.Problem.ToActionResult(); // This line will now work with the extension method
            }

            return Ok(ConsentInfoMaskinporten.Convert(consent.Value));
        }

        private ConsentPartyUrn MapToCore(ConsentPartyUrnExternal consentPartyUrnExternal)
        {
            ConsentPartyUrn consentPartyUrn = consentPartyUrnExternal switch
            {
                ConsentPartyUrnExternal.PersonId => ConsentPartyUrn.PersonId.Create(PersonIdentifier.Parse(consentPartyUrnExternal.ValueSpan)),
                ConsentPartyUrnExternal.OrganizationId => ConsentPartyUrn.OrganizationId.Create(OrganizationNumber.Parse(consentPartyUrnExternal.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return consentPartyUrn;
        }
    }
}
