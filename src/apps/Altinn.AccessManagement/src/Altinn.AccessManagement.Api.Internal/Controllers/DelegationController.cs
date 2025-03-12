using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    /// <summary>
    /// Delegations
    /// </summary>
    [Route("/accessmanagement/api/v1/delegations")]
    [ApiController]
    public class DelegationController(
        IDelegationRepository delegationRepository,
        IDelegationPackageRepository delegationPackageRepository,
        IDelegationResourceRepository delegationResourceRepository,
        IAssignmentRepository assignmentRepository
        ) : ControllerBase
    {
        private readonly IDelegationRepository delegationRepository = delegationRepository;
        private readonly IDelegationPackageRepository delegationPackageRepository = delegationPackageRepository;
        private readonly IDelegationResourceRepository delegationResourceRepository = delegationResourceRepository;
        private readonly IAssignmentRepository assignmentRepository = assignmentRepository;

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("")]
        [HttpPost]
        public async Task<ActionResult> CreateDelegation([FromBody] CreateDelegationRequestDto request)
        {
            var fromAssignment = await assignmentRepository.Get(request.FromAssignmentId);
            var toAssignment = await assignmentRepository.Get(request.ToAssignmentId);

            if (fromAssignment == null || toAssignment == null)
            {
                // Assignments must exist
                return BadRequest();
            }

            if (!fromAssignment.ToId.Equals(toAssignment.FromId))
            {
                // Assignments must have a link
                return BadRequest();
            }

            var filter = delegationRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, fromAssignment.Id);
            filter.Equal(t => t.ToId, toAssignment.Id);
            filter.Equal(t => t.FacilitatorId, fromAssignment.ToId);
            var existsRes = (await delegationRepository.Get(filter)).FirstOrDefault();
            if (existsRes != null)
            {
                // Allready exists
                return Ok(); // 302 Found?
            }

            var res = await delegationRepository.Create(new AccessMgmt.Core.Models.Delegation()
            {
                Id = Guid.NewGuid(),
                FromId = request.FromAssignmentId,
                ToId = request.ToAssignmentId,
                FacilitatorId = Guid.Empty
            });
            if (res > 0)
            {
                // Created
                return Created();
            }

            return Problem();
        }

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("/flat")]
        [HttpPost]
        public async Task<ActionResult> CreateFlatDelegation([FromBody] CreateDelegationFlatRequestDto request)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            //var user = (await entityRepository.Get(userId)) ?? throw new Exception(string.Format("Party not found '{0}'", userId));

            return Problem();
        }

        /// <summary>
        /// Remove package from delegation
        /// </summary>
        [Route("{id}")]
        [HttpDelete]
        public async Task<ActionResult> CreateDelegation(Guid id)
        {
            /*
           - User must have role:TS on Facilitator
           */

            try
            {
                var res = await delegationRepository.Delete(id);
                if (res > 0)
                {
                    return NoContent();
                }

                throw new Exception("Unable to delete delegation");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Add package to delegation
        /// </summary>
        [Route("{id}/package/{packageId}")]
        [HttpPost]
        public async Task<ActionResult> AddPackage(Guid id, Guid packageId)
        {
            /*
            
            - User must have role:TS on Facilitator
            - Package must exist in /connection/{assignment_id}/packages
            - Package must be delegable

            */

            try
            {
                var res = await delegationPackageRepository.CreateCross(id, packageId);
                if (res > 0)
                {
                    return Created();
                }

                throw new Exception("Unable to add package to delegation");
            }
            catch (Exception ex) 
            {
                return Problem(ex.Message);
            }
        }

        /// <summary>
        /// Remove package from delegation
        /// </summary>
        [Route("{id}/package/{packageId}")]
        [HttpDelete]
        public async Task<ActionResult> RemovePackage(Guid id, Guid packageId)
        {
            /*

           - User must have role:TS on Facilitator
           - Package should exist in /connection/{delegation_id}/packages

           */

            try
            {
                var res = await delegationPackageRepository.DeleteCross(id, packageId);
                if (res > 0)
                {
                    return NoContent();
                }

                throw new Exception("Unable to remove package from delegation");
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

    }
}

public class CreateDelegationRequestDto
{
    public Guid FromAssignmentId { get; set; }
    public Guid ToAssignmentId { get; set; }
}

public class CreateDelegationFlatRequestDto
{
    public Guid ClientPartyId { get; set; }
    public Guid FacilitatorPartyId { get; set; }
    public string ClientRole { get; set; }
    public Guid AgentPartyId { get; set; }
    public Guid AgentName { get; set; }
    public string AgentRole { get; set; }
    public string[] Packages { get; set; }
}

/*
// Do we need a RequestDto?  
public class AddDelegationPackageRequestDto
{
    public Guid DelegationId { get; set; }
    public Guid PackageId { get; set; }
}
*/
