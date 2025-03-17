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
            List<AccessPackageUrn> packages = new();

            // ToDo: This is a temporary implementation to return a list of access packages for a given from and to party
            if (to.ToString() == "e2eba2c3-b369-4ff9-8418-99a810d6bb58" && (from.ToString() == "066148fe-7077-4484-b7ea-44b5ede0014e" || from.ToString() == "825d14bf-b3f3-4d68-ae33-0994febf8a43"))
            {
                // Scenario: Direct delegation from main unit owning the system user. Delegation of package: ansettelsesforhold, is expected to be found when from is either the owning main unit or its sub units
                packages = new List<AccessPackageUrn>
                {
                    AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("ansettelsesforhold"))
                };
            }
            else if (to.ToString() == "e2eba2c3-b369-4ff9-8418-99a810d6bb58" && (from.ToString() == "c12f8f37-391b-4651-be09-05665f5acdb6" || from.ToString() == "86ae6d6a-3545-4956-b395-c67ca0df4e51"))
            {
                // Scenario: Client delegation from a main unit client of the system user of the accountant unit. Delegation of package: regnskapsforer-med-signeringsrettighet, is expected to be found when from is either the client main unit or its sub units
                packages = new List<AccessPackageUrn>
                {
                    AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("regnskapsforer-med-signeringsrettighet"))
                };
            }
            else if (to.ToString() == "e2eba2c3-b369-4ff9-8418-99a810d6bb58" && (from.ToString() == "ab07bec2-fcd0-4563-908a-d9f564724252" || from.ToString() == "00273506-3b4a-4e8e-a1f7-b7f28c4b411b"))
            {
                // Scenario: Client delegation from Enkeltpersonforetak to the system user of the accountant unit. Delegation of package: regnskapsforer-lonn, is expected to be found when from is either the client ENK or its Innehaver
                packages = new List<AccessPackageUrn>
                {
                    AccessPackageUrn.AccessPackageId.Create(AccessPackageIdentifier.CreateUnchecked("regnskapsforer-lonn"))
                };
            }

            return Ok(packages);
        }
    }
}
