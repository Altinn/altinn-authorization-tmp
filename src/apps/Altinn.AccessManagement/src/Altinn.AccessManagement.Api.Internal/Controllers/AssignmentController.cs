using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    /// <summary>
    /// Assignments
    /// </summary>
    [Route("/accessmanagement/api/v1/assignments")]
    [ApiController]
    public class AssignmentController(
        IConnectionRepository connectionRepository,
        IConnectionPackageRepository connectionPackageRepository,
        IPackageRepository packageRepository,
        IEntityRepository entityRepository,
        IAssignmentRepository assignmentRepository,
        IAssignmentPackageRepository assignmentPackageRepository,
        IAssignmentResourceRepository assignmentResourceRepository,
        IProviderRepository providerRepository,
        IRoleRepository roleRepository
        ) : ControllerBase
    {
        private readonly IConnectionRepository connectionRepository = connectionRepository;
        private readonly IConnectionPackageRepository connectionPackageRepository = connectionPackageRepository;
        private readonly IPackageRepository packageRepository = packageRepository;
        private readonly IEntityRepository entityRepository = entityRepository;
        private readonly IAssignmentRepository assignmentRepository = assignmentRepository;
        private readonly IAssignmentPackageRepository assignmentPackageRepository = assignmentPackageRepository;
        private readonly IAssignmentResourceRepository assignmentResourceRepository = assignmentResourceRepository;
        private readonly IProviderRepository providerRepository = providerRepository;
        private readonly IRoleRepository roleRepository = roleRepository;

        /// <summary>
        /// Create a new assignment
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Post([FromBody] Assignment assignment)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var userEntity = await entityRepository.Get(userId);
            var fromEntity = await entityRepository.Get(assignment.FromId);
            var toEntity = await entityRepository.Get(assignment.ToId);
            var role = await roleRepository.Get(assignment.RoleId);

            if (userEntity == null || fromEntity == null || toEntity == null || role == null)
            {
                return BadRequest();
            }

            var filter = connectionRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, fromEntity.Id);
            filter.Equal(t => t.ToId, userEntity.Id);
            var connections = await connectionRepository.GetExtended(filter);

            string roleUrn = "urn:altinn:role:hovedadministrator"; // Or something else?
            if (connections.Count(t => t.Role.Urn == roleUrn) == 0)
            {
                return Unauthorized(string.Format("User '{0}' is missing role '{1}' on '{2}'", userId, roleUrn, assignment.FromId.ToString()));
            }

            await assignmentRepository.Create(new Assignment()
            {
                Id = Guid.NewGuid(),
                FromId = fromEntity.Id,
                ToId = toEntity.Id,
                RoleId = role.Id,
            });

            return Created();
        }

        /// <summary>
        /// Delete assignment
        /// </summary>
        [HttpDelete]
        [Authorize]
        public async Task<ActionResult> Delete(Guid id)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var digdirProvider = (await providerRepository.Get(t => t.Name, "Digitaliseringsdirektoratet")).FirstOrDefault();
            if (digdirProvider == null)
            {
                return Problem("Unable to find provider");
            }

            var assignment = await assignmentRepository.GetExtended(id);
            if (assignment.Role.ProviderId != digdirProvider.Id)
            {
                return Problem("Not allowed to delete this assignment");
            }

            await assignmentRepository.Delete(id);
            return Ok();
        }

        /// <summary>
        /// Alle relasjoner hvor {id} er involvert i from, to eller facilitator.
        /// </summary>
        [Route("{id}/packages/{packageId}")]
        [HttpPost]
        public async Task<ActionResult> AddPackage(Guid id, Guid packageId)
        {
            var userId = AuthenticationHelper.GetPartyUuid(HttpContext);
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var userEntity = await entityRepository.Get(userId);
            var assignment = await assignmentRepository.Get(id);
            var package = await packageRepository.Get(packageId);

            if (userEntity == null || assignment == null || package == null)
            {
                return BadRequest();
            }

            if (!package.IsAssignable)
            {
                return Problem("Package is not available for assignment");
            }

            var filter = connectionRepository.CreateFilterBuilder();
            filter.Equal(t => t.FromId, assignment.FromId);
            filter.Equal(t => t.ToId, userEntity.Id);
            var connections = await connectionRepository.GetExtended(filter);

            string roleUrn = "urn:altinn:role:tilgangsstyrer"; // Or something else?
            if (connections.Count(t => t.Role.Urn == roleUrn) == 0)
            {
                return Unauthorized(string.Format("User '{0}' is missing role '{1}' on '{2}'", userId, roleUrn, assignment.FromId.ToString()));
            }

            var availablePackages = new List<Package>();
            foreach (var con in connections)
            {
                availablePackages.AddRange(await connectionPackageRepository.GetB(con.Id));
            }

            if (availablePackages.Count(t => t.Id == package.Id) == 0)
            {
                return Problem("USer dows not have the package available for assignment");
            }

            var assPckFilter = assignmentPackageRepository.CreateFilterBuilder();
            assPckFilter.Equal(t => t.AssignmentId, assignment.Id);
            assPckFilter.Equal(t => t.PackageId, package.Id);
            var assPck = assignmentPackageRepository.Get(assPckFilter);
            if (assPckFilter != null && assPckFilter.Any())
            {
                return Created(); // Allready exists
            }

            /*
            
            - [x] User exists
            - [x] User is TS
            - [x] Package exists
            - [x] User has package
            - [x] Assignment exists
            - [x] AssignmentPackage does not exist (duplicate)

            */

            var dp = new AssignmentPackage()
            {
                Id = Guid.NewGuid(),
                AssignmentId = id,
                PackageId = packageId
            };

            var res = await assignmentPackageRepository.CreateCross(assignment.Id, package.Id);
            if (res == 1)
            {
                return Created();
            }

            return Problem("Unable to add package to assignment");

        }
    }
}
