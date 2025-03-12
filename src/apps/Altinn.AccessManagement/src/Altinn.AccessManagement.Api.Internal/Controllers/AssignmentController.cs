using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Internal.Controllers
{
    /// <summary>
    /// Assignments
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentController(IAssignmentRepository assignmentRepository) : ControllerBase
    {
        private readonly IAssignmentRepository assignmentRepository = assignmentRepository;

        /*
Get Assignment
Legg til eller fjern pakker og ressurser
*/
        [HttpPost]
        public async Task Post([FromBody] Assignment assignment)
        {


            await assignmentRepository.Create(new Assignment()
            {
                Id = Guid.NewGuid(),
                FromId = Guid.NewGuid(),
                ToId = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
            });
        }

        [HttpDelete]
        public async Task Delete(Guid id)
        {
            await assignmentRepository.Delete(id);
        }
    }
}
