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
        /// Gets all <see cref="VariantDto"/> for given type
        /// </summary>
        /// [Route("{typeName}/types")]
        /// [HttpGet]
        private async Task<ActionResult<List<VariantDto>>> GetTypeVariants(string typeName)
        {
            if (!EntityTypeConstants.TryGetByName(typeName, out var entityType))
            {
                return NotFound($"Type {typeName} not found. Try 'Organisasjon'");
            }

            return EntityVariantConstants.AllEntities().Where(t => t.Entity.TypeId == entityType.Id).Select(t => DtoMapper.ConvertFlat(t.Entity)).ToList();
        }

        /// <summary>
        /// Gets all organization variants <see cref="VariantDto"/>
        /// </summary>
        [Route("organization/types")]
        [HttpGet]
        public async Task<ActionResult<List<VariantDto>>> GetOrganizationVariants()
        {
            return EntityVariantConstants.AllEntities().Where(t => t.Entity.TypeId == EntityTypeConstants.Organization.Id).Select(t => DtoMapper.ConvertFlat(t.Entity)).ToList();
        }
    }
}
