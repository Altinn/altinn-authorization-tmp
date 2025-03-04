using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Utilities.Search;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [Route("/accessmanagement/api/v1/meta/info/accesspackages/")]
    [ApiController]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageService packageService;

        public PackagesController(IPackageService packageService) 
        {
            this.packageService = packageService;
        }

        [Route("search/{term}")]
        [HttpGet]
        public async Task<IEnumerable<SearchObject<PackageDto>>> Search(string term)
        {
            return await packageService.Search(term);
        }

        [Route("export")]
        [HttpGet]
        public async Task<IEnumerable<AreaGroupDto>> GetHierarchy()
        {
            return await packageService.GetHierarchy();
        }

        [Route("group/")]
        [HttpGet]
        public async Task<IEnumerable<AreaGroup>> GetGroups()
        {
            return await packageService.GetAreaGroups();
        }

        [Route("group/{id}")]
        [HttpGet]
        public async Task<AreaGroup> GetGroup(Guid id)
        {
            return await packageService.GetAreaGroup(id);
        }

        [Route("group/{id}/areas")]
        [HttpGet]
        public async Task<IEnumerable<Area>> GetGroupAreas(Guid id)
        {
            return await packageService.GetAreas(id);
        }

        [Route("area/{id}")]
        [HttpGet]
        public async Task<Area> GetArea(Guid id)
        {
            return await packageService.GetArea(id);
        }

        [Route("area/{id}/packages")]
        [HttpGet]
        public async Task<IEnumerable<PackageDto>> GetAreaPackages(Guid id)
        {
            return await packageService.GetPackagesByArea(id);
        }

        [Route("package/{id}")]
        [HttpGet]
        public async Task<PackageDto> GetPackage(Guid id)
        {
            return await packageService.GetPackage(id);
        }

        [Route("package/{id}/resources")]
        [HttpGet]
        public async Task<IEnumerable<Resource>> GetPackageResources(Guid id)
        {
            return await packageService.GetPackageResources(id);
        }
    }
}
