using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    /// <summary>
    /// Endpoint to expose Connections
    /// Connections are a combination of Assignments and Delegations
    /// </summary>
    [Route("/accessmanagement/api/v1/connections")]
    [ApiController]
    public class ConnectionController(
        IConnectionRepository connectionRepository,
        IConnectionPackageRepository connectionPackageRepository,
        IConnectionResourceRepository connectionResourceRepository
        ) : ControllerBase
    {
        private readonly IConnectionRepository connectionRepository = connectionRepository;
        private readonly IConnectionPackageRepository connectionPackageRepository = connectionPackageRepository;
        private readonly IConnectionResourceRepository connectionResourceRepository = connectionResourceRepository;

        /// <summary>
        /// Alle enheter {id} har gitt tilgang til.
        /// </summary>
        [Route("given/{id}")]
        [HttpGet]
        public async Task<ActionResult<ExtConnection>> GetGiven(Guid id)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var res = await connectionRepository.GetExtended(t => t.FromId, id);

            return Ok(res);
        }

        /// <summary>
        /// Alle enheter {id} har fått tilgang fra.
        /// </summary>
        [Route("recived/{id}")]
        [HttpGet]
        public async Task<ActionResult<ExtConnection>> GetRecived(Guid id)
        {
            var res = await connectionRepository.GetExtended(t => t.ToId, id);

            return Ok(res);
        }

        /// <summary>
        /// Alle enheter der {id} fasiliterer en delegering.
        /// </summary>
        [Route("facilitated/{id}")]
        [HttpGet]
        public async Task<ActionResult<ExtConnection>> GetFacilitated(Guid id)
        {
            var res = await connectionRepository.GetExtended(t => t.FacilitatorId, id);

            return Ok(res);
        }

        /// <summary>
        /// Alle relasjoner hvor {id} er involvert i from, to eller facilitator.
        /// </summary>
        [Route("all/{id}")]
        [HttpGet]
        public async Task<ActionResult<ExtConnection>> GetAll(Guid id)
        {
            var resGiven = await connectionRepository.GetExtended(t => t.FromId, id);
            var resRecived = await connectionRepository.GetExtended(t => t.ToId, id);
            var resFacilitated = await connectionRepository.GetExtended(t => t.FacilitatorId, id);

            var res = new List<ExtConnection>();
            res.AddRange(resGiven);
            res.AddRange(resRecived);
            res.AddRange(resFacilitated);

            return Ok(res);
        }

        /// <summary>
        /// Alle relasjoner hvor {id} er involvert i from, to eller facilitator.
        /// </summary>
        [Route("{fromid}/{toid}")]
        [HttpGet]
        public async Task<ActionResult<ExtConnection>> GetSpecific(Guid fromId, Guid toId)
        {
            var filter = connectionRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, fromId);
            filter.Equal(t => t.ToId, toId);
            var res = await connectionRepository.GetExtended(filter);

            return Ok(res);
        }

        /// <summary>
        /// Alle relasjoner hvor {id} er involvert i from, to eller facilitator.
        /// </summary>
        [Route("{id}/packages")]
        [HttpGet]
        public async Task<ActionResult<Package>> GetPackages(Guid id)
        {
            return Ok(await connectionPackageRepository.GetB(id));
        }

        /// <summary>
        /// Alle relasjoner hvor {id} er involvert i from, to eller facilitator.
        /// </summary>
        [Route("{id}/resources")]
        [HttpGet]
        public async Task<ActionResult<Resource>> GetResources(Guid id)
        {
            return Ok(await connectionResourceRepository.GetB(id));
        }
    }
}
