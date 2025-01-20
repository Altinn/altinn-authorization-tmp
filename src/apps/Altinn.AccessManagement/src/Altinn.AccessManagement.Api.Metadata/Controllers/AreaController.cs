using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [ApiController]
    public class AreaController(IAreaService service) : ControllerBase
    {
        private readonly IAreaService service = service;

        [Route("/accessmanagement/api/v1/metadata/[controller]")]
        [HttpGet]
        public async Task<ActionResult<Area>> GetAll()
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
        public async Task<ActionResult<ExtArea>> Get(Guid id)
        {
            try
            {
                var result = await service.GetExtended(id);
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

        [Route("/accessmanagement/api/v1/metadata/AreaGroups/{id}/areas")]
        [HttpGet]
        public async Task<ActionResult<Area>> GetByGroup(Guid id)
        {
            try
            {
                var result = await service.Get(t => t.GroupId, id);
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
