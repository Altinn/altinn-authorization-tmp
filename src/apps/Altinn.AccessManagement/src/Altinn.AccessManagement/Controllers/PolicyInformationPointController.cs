using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Rights;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.AccessManagement.Models;
using Altinn.Authorization.Models;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Controllers
{
    /// <summary>
    /// Controller responsible for all operations for managing delegations of Altinn Apps
    /// </summary>
    [Route("accessmanagement/api/v1/policyinformation")]
    [ApiController]
    public class PolicyInformationPointController : ControllerBase
    {
        private readonly IPolicyInformationPoint _pip;
        private readonly IMapper _mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyInformationPointController"/> class.
        /// </summary>
        /// <param name="pip">The policy information point</param>
        /// <param name="mapper">The mapper</param>
        public PolicyInformationPointController(IPolicyInformationPoint pip, IMapper mapper)
        {
            _pip = pip;
            _mapper = mapper;
        }

        /// <summary>
        /// Endpoint to find all delegation changes for a given user, reportee and app/resource context
        /// </summary>
        /// <param name="request">The input model that contains id info about user, reportee, resource and resourceMatchType </param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>A list of delegation changes that's stored in the database </returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost]
        [Route("getdelegationchanges")]
        public async Task<ActionResult<List<DelegationChangeExternal>>> GetAllDelegationChanges([FromBody] DelegationChangeInput request, CancellationToken cancellationToken)
        {
            DelegationChangeList response = await _pip.GetAllDelegations(request, includeInstanceDelegations: true, cancellationToken);

            if (!response.IsValid)
            {
                foreach (string errorKey in response.Errors.Keys)
                {
                    ModelState.AddModelError(errorKey, response.Errors[errorKey]);
                }

                return new ObjectResult(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
            }

            return _mapper.Map<List<DelegationChangeExternal>>(response.DelegationChanges);
        }

        /// <summary>
        /// Endpoint to lookup all access packages a given to-party uuid has for a given from-party uuid
        /// </summary>
        /// <param name="from">The uuid of the party to lookop if the to-party has access packages for</param>
        /// <param name="to">The uuid of the party to lookup access packages og behalf of the from-party</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns>A list of all access package urns to-party has access to on behalf of the from-party</returns>
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("accesspackages")]
        public async Task<ActionResult> GetAccessPackages([FromQuery] Guid from, [FromQuery] Guid to, CancellationToken cancellationToken)
        {
            // ToDo: This is a temporary implementation to return a list of access packages for a given from and to party
            var packages = new List<AccessPackageUrn>
            {
                AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("skatt-naering")),
                AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("ansettelsesforhold")),
                AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("maskinporten-scopes"))
            };

            return Ok(packages);
        }
    }
}
