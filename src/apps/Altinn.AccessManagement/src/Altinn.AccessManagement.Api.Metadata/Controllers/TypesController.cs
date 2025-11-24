using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessManagement.Api.Metadata.Controllers
{
    [Route("/accessmanagement/api/v1/meta/info/types/")]
    [ApiController]
    public class TypesController : ControllerBase
    {
        private readonly AppDbContext dbContext;

        public TypesController(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        /// <summary>
        /// Gets all <see cref="EntityTypeDto"/>
        /// </summary>
        /// [Route("")]
        /// [HttpGet]
        private async Task<ActionResult<List<EntityTypeDto>>> GetAllTypes()
        {
            var res = await dbContext.EntityTypes.AsNoTracking().ToListAsync();
            if (res == null)
            {
                return NotFound();
            }

            var dtos = res.Select(DtoMapper.Convert);

            return Ok(dtos);
        }

        /// <summary>
        /// Gets all <see cref="VariantDto"/> for given type
        /// </summary>
        /// [Route("{typeName}/variants")]
        /// [HttpGet]
        private async Task<ActionResult<List<VariantDto>>> GetTypeVariants(string typeName)
        {
            if (!EntityTypeConstants.TryGetByName(typeName, out var entityType))
            {
                return NotFound($"Type {typeName} not found. Try 'Organisasjon'");
            }

            var res = await dbContext.EntityVariants.AsNoTracking().Where(t => t.TypeId == entityType.Id).ToListAsync();
            if (res == null)
            {
                return NotFound();
            }

            var dtos = res.Select(DtoMapper.ConvertFlat);

            return Ok(dtos);
        }

        /// <summary>
        /// Gets all organization variants <see cref="VariantDto"/>
        /// </summary>
        [Route("organisasjon/variants")]
        [HttpGet]
        public async Task<ActionResult<List<VariantDto>>> GetOrganizationVariants()
        {
            var res = await dbContext.EntityVariants.AsNoTracking().Where(t => t.TypeId == EntityTypeConstants.Organisation.Id).ToListAsync();
            if (res == null)
            {
                return NotFound();
            }

            var dtos = res.Select(DtoMapper.ConvertFlat);

            return Ok(dtos);
        }
    }
}
