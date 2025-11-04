using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    /// <summary>
    /// Metadata endpoints for Roles 
    /// </summary>
    [Route("/accessmanagement/api/v1/meta/info/roles/")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService roleService;

        /// <summary>
        /// Initialiserer en ny instans av <see cref="RolesController"/>.
        /// </summary>
        /// <param name="roleService">Service for håndtering av roller.</param>
        public RolesController(IRoleService roleService)
        {
            this.roleService = roleService;
        }

        /// <summary>
        /// Gets all <see cref="RoleDto"/>
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Route("")]
        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            var res = await roleService.GetAll();
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
            var res = await roleService.GetById(id);
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
            var res = await roleService.GetByKeyValue(key, value);
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
            var res = await roleService.GetLookupKeys();
            if (res == null)
            {
                return NotFound();
            }

            return Ok(res);
        }

        /// <summary>
        /// Gets privledges for a specific role and variant combination.
        /// </summary>
        [HttpGet("privledges")]
        [ProducesResponseType(typeof(RoleVariantPrivilegeDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<RoleVariantPrivilegeDto>> GetRoleVariantPrivledges([FromQuery] string role, [FromQuery] string variant)
        {
            RoleConstants.TryGetByCode(string.IsNullOrEmpty(role) ? "_" : role, out var roleDef);
            EntityVariantConstants.TryGetByName(string.IsNullOrEmpty(variant) ? "_" : variant, out var variantDef);

            var result = await roleService.GetPrivileges(roleId: roleDef == null ? null : roleDef.Id, variantId: variantDef == null ? null : variantDef.Id);
            return Ok(result);
        }

        /// <summary>
        /// Gets privledges for a specific role.
        /// </summary>
        [HttpGet("privledges/role")]
        [ProducesResponseType(typeof(IEnumerable<VariantPrivilegeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VariantPrivilegeDto>>> GetRolePrivledges([FromQuery] string role)
        {
            if (!RoleConstants.TryGetByCode(role, out var roleDef))
            {
                return BadRequest($"Role '{role}' not found");
            }

            var result = await roleService.GetPrivileges(roleId: roleDef.Id, variantId: null);
            return Ok(result.Select(DtoMapper.ConvertToVariantPrivledgeDto));
        }

        /// <summary>
        /// Gets privledges for a specific variant.
        /// </summary>
        [HttpGet("privledges/variant")]
        [ProducesResponseType(typeof(IEnumerable<RolePrivilegeDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RolePrivilegeDto>>> GetVariantPrivledges([FromQuery] string variant)
        {
            if (!EntityVariantConstants.TryGetByName(variant, out var variantDef))
            {
                return BadRequest($"Variant '{variant}' not found");
            }

            var result = await roleService.GetPrivileges(roleId: null, variantId: variantDef.Id);
            return Ok(result.Select(DtoMapper.ConvertToRolePrivledgeDto));
        }
    }
}

