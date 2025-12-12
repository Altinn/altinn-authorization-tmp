using Altinn.AccessMgmt.Core.Services.Legacy;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [Route("/accessmanagement/api/v1/meta/info/test/")]
    [ApiController]
    public class ValuesController(IDelegationChangesService changesService) : ControllerBase
    {
        [Route("testing")]
        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            var changes = await changesService.GetDelegations();
            return changes.Select(t => t.BlobStoragePolicyPath);
        }
    }
}
