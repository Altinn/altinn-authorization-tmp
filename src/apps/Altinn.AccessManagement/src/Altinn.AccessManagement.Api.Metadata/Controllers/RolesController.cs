using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    /// <summary>
    /// Metadata endpoints for Roles 
    /// </summary>
    [Route("/accessmanagement/api/v1/meta/info/roles/")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly Altinn.AccessMgmt.Core.Services.Contracts.IRoleService coreRoleService;
        private readonly Altinn.AccessMgmt.Persistence.Services.Contracts.IRoleService persistenceRoleService;
        private readonly IFeatureManager featureManager;

        /// <summary>
        /// Initialiserer en ny instans av <see cref="RolesController"/>.
        /// </summary>
        /// <param name="roleService">Service for håndtering av roller.</param>
        public RolesController(
            Altinn.AccessMgmt.Core.Services.Contracts.IRoleService coreRoleService,
            Altinn.AccessMgmt.Persistence.Services.Contracts.IRoleService persistenceRoleService,
            IFeatureManager featureManager)
        {
            this.coreRoleService = coreRoleService;
            this.persistenceRoleService = persistenceRoleService;
            this.featureManager = featureManager;
        }

        /// <summary>
        /// Gets all <see cref="RoleDto"/>
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("")]
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            IEnumerable<RoleDto> res;
            if (await featureManager.IsEnabledAsync("AccessMgmt.RoleService.EFCore"))
            {
                res = await coreRoleService.GetAll();
            }
            else
            {
                res = await persistenceRoleService.GetAll();
            }
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        /// <summary>
        /// Gets <see cref="RoleDto"/> with id
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("{id}")]
        [HttpGet]
        public async Task<ActionResult<RoleDto>> GetId(Guid id)
        {
            RoleDto res;
            if (await featureManager.IsEnabledAsync("AccessMgmt.RoleService.EFCore"))
            {
                res = await coreRoleService.GetById(id);
            }
            else
            {
                res = await persistenceRoleService.GetById(id);
            }
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        /// <summary>
        /// Gets <see cref="RoleDto"/> with a key/pair value
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("lookup")]
        [HttpGet]
        public async Task<ActionResult<RoleDto>> GetKeyPairValue([FromQuery] string key, [FromQuery] string value)
        {
            RoleDto res;
            if (await featureManager.IsEnabledAsync("AccessMgmt.RoleService.EFCore"))
            {
                res = await coreRoleService.GetByKeyValue(key, value);
            }
            else
            {
                res = await persistenceRoleService.GetByKeyValue(key, value);
            }
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        /// <summary>
        /// Gets possible lookup keys for roles
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("lookup/keys")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> GetKeys()
        {
            IEnumerable<string> res;
            if (await featureManager.IsEnabledAsync("AccessMgmt.RoleService.EFCore"))
            {
                res = await coreRoleService.GetLookupKeys();
            }
            else
            {
                res = await persistenceRoleService.GetLookupKeys();
            }
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }

        /// <summary>
        /// Gets all <see cref="RolePackageDto"/> for <see cref="RoleDto"/>
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("{id}/packages")]
        [HttpGet]
        public async Task<ActionResult<List<RolePackageDto>>> GetPackagesForRole(Guid id)
        {
            IEnumerable<RolePackageDto> res;
            if (await featureManager.IsEnabledAsync("AccessMgmt.RoleService.EFCore"))
            {
                res = await coreRoleService.GetPackagesForRole(id);
            }
            else
            {
                res = await persistenceRoleService.GetPackagesForRole(id);
            }
            if (res == null)
            {
                return NotFound();
            }
            return Ok(res);
        }
    }
}
