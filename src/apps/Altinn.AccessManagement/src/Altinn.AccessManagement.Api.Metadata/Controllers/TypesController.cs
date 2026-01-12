using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [Route("/accessmanagement/api/v1/meta/types")]
    [ApiController]
    public class TypesController : ControllerBase
    {
        /// <summary>
        /// Gets all <see cref="EntityTypeDto"/>
        /// </summary>
        /// [Route("")]
        /// [HttpGet]
        private async Task<ActionResult<List<EntityTypeDto>>> GetAllTypes()
        {
            return EntityTypeConstants.AllEntities().Select(t => DtoMapper.Convert(t.Entity)).ToList();
        }

        /// <summary>
        /// Gets all <see cref="EntityTypeDto"/> for given type
        /// </summary>
        /// [Route("{parentTypeName}/subtypes")]
        /// [HttpGet]
        private async Task<ActionResult<List<EntitySubTypeDto>>> GetSubTypes(string parentTypeName)
        {
            if (!EntityTypeConstants.TryGetByName(parentTypeName, out var entityType))
            {
                return NotFound($"Type {parentTypeName} not found. Try 'organization'");
            }

            return EntityVariantConstants.AllEntities().Where(t => t.Entity.TypeId == entityType.Id).Select(t => DtoMapper.ConvertFlat(t.Entity)).ToList();
        }

        /// <summary>
        /// Gets all organization variants <see cref="EntitySubTypeDto"/>
        /// </summary>
        [Route("organization/subtypes")]
        [HttpGet]
        public async Task<ActionResult<List<EntitySubTypeDto>>> GetOrganizationSubTypes()
        {
            return EntityVariantConstants.AllEntities().Where(t => t.Entity.TypeId == EntityTypeConstants.Organization.Id).Select(t => DtoMapper.ConvertFlat(t.Entity)).ToList();
        }
    }
}
