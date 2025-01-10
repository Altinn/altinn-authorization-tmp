using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.AccessPackages.Models;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [ApiController]
    public class AreaGroupController(IAreaGroupService service) : ControllerBase
    {
        private readonly IAreaGroupService service = service;

        [Route("api/[controller]")]
        [HttpGet]
        public async Task<ActionResult<AreaGroup>> GetAll()
        {
            try
            {
                var result = await service.Get();
                if (result == null || !result.Any())
                {
                    return NotFound();
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        [Route("api/[controller]/{id}")]
        [HttpGet]
        public async Task<ActionResult<AreaGroup>> Get(Guid id)
        {
            try
            {
                var result = await service.Get(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Problem(ex.Message);
            }
        }
    }
}
