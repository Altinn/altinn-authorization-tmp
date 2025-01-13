using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [ApiController]
    public class PackageController(IPackageService service) : ControllerBase
    {
        private readonly IPackageService service = service;

        [Route("api/[controller]")]
        [HttpGet]
        public async Task<ActionResult<Package>> GetAll()
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
        public async Task<ActionResult<ExtPackage>> Get(Guid id)
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

        [Route("api/Area/{id}/Packages")]
        [HttpGet]
        public async Task<ActionResult<Package>> GetByGroup(Guid id)
        {
            try
            {
                var result = await service.Get(t => t.AreaId, id);
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
