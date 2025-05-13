using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Maskinporten.Controllers
{
    /// <summary>
    /// Consent API for Maskinporten. Used to lookup consent information.
    /// </summary>
    /// <remarks>
    /// The default constructor taking in depencies. 
    /// </remarks>
    [Route("accessmanagement/api/v1/maskinporten/consent/")]
    [ApiController]
    public class ConsentController(IConsent consentService) : ControllerBase
    {
        private readonly IConsent _consentService = consentService;

        /// <summary>
        /// Returns a specific consent based on consent id an from party
        /// </summary>
        [HttpPost]
        [Route("lookup")]
        [Authorize(Policy = AuthzConstants.POLICY_MASKINPORTEN_CONSENT_READ)]
        public async Task<ActionResult<ConsentInfoMaskinporten>> GetConsent([FromBody] ConsentLookup consentLookup, CancellationToken cancellationToken = default)
        {
            Result<Consent> consent = await _consentService.GetConsent(consentLookup.Id, MapToCore(consentLookup.From), MapToCore(consentLookup.To), cancellationToken);

            if (consent.IsProblem)
            {
                return consent.Problem.ToActionResult();
            }

            return Ok(ConsentInfoMaskinporten.Convert(consent.Value));
        }

        private static ConsentPartyUrn MapToCore(ConsentPartyUrnExternal consentPartyUrnExternal)
        {
            ConsentPartyUrn consentPartyUrn = consentPartyUrnExternal switch
            {
                ConsentPartyUrnExternal.PersonId personUrn => ConsentPartyUrn.PersonId.Create(personUrn.Value),
                ConsentPartyUrnExternal.OrganizationId organizationUrn => ConsentPartyUrn.OrganizationId.Create(organizationUrn.Value),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return consentPartyUrn;
        }
    }
}
