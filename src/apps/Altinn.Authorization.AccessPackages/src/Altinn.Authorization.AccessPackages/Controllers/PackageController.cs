using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.AccessPackages.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PackageController(IPackageService service) : ControllerBase
    {
        private readonly IPackageService service = service;

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
    }
}
