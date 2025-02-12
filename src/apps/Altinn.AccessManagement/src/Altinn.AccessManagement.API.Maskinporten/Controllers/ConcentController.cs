using Altinn.AccessManagement.Api.Maskinporten.Models.Concent;
using Altinn.Register.Core.Parties;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Maskinporten.Controllers
{
    /// <summary>
    /// Comcent controller for Maskinporten
    /// </summary>
    [Route("/accessmanagment/api/maskinporten")]
    [ApiController]
    public class ConcentController : ControllerBase
    {
        /// <summary>
        /// 
        /// </summary>
        [HttpGet("/concent/lookup")]
        public ActionResult<ConsentInfoMaskinporten> GetConcent(Guid id, string from, string to)
        {
            ConsentInfoMaskinporten consentInfoMaskinporten = new ConsentInfoMaskinporten()
            {
                Id = id,
                To = ConsentPartyUrn.PersonId.Create.Create(PersonIdentifier.Parse("01038712345")),
                From = ConsentPartyUrn.OrganizationId.Create(Core.Models.Register.OrganizationNumber.Parse("123456789")),
                ConcentRights = new List<ConsentRightExternal>() 
            };

            return Ok(consentInfoMaskinporten);
        }
    }
}
