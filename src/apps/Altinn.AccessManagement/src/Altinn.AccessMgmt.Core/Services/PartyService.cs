using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Models;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.ProblemDetails;
using Altinn.Authorization.Api.Contracts.Party;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc />
public class PartyService(AppDbContext dbContext, DtoConverter dtoConverter) : IPartyService
{
    /// <inheritdoc />
    public async Task<Result<AddPartyResultDto>> AddParty(PartyBaseInternal party, ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        AddPartyResultDto result = new() { PartyUuid = party.PartyUuid, PartyCreated = false };

        var exists = await dbContext.Entities
            .AnyAsync(e => e.Id == party.PartyUuid, cancellationToken);

        if (!exists)
        {
            /*
             * Info: For now this is restricted to SystemUser as it does not have any logic to set the correct RefId based on entity type Organisation number for Organisation and F�dselsnummer for Person. 
             * Also there should be a matching record in the EntityLookup table for required lookup values SystemUSer does not have any lookup values as the only id it has is its PartyUuid.
             */
            if (!party.EntityType.Equals("Systembruker", StringComparison.InvariantCultureIgnoreCase))
            {
                return Problems.UnsuportedEntityType.Create([new("entityType", party.EntityType.ToString())]);
            }

            var entityType = await dbContext.EntityTypes
                .FirstOrDefaultAsync(t => t.Name == party.EntityType, cancellationToken);

            if (entityType == null)
            {
                return Problems.EntityTypeNotFound.Create([new("entityType", party.EntityType)]);
            }

            var entityVariant = await dbContext.EntityVariants
                .Where(t => t.TypeId == entityType.Id && t.Name.Equals(party.EntityVariantType, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefaultAsync(cancellationToken);

            if (entityVariant == null)
            {
                return Problems.EntityVariantNotFoundOrInvalid.Create([new("entityVariantId", party.EntityVariantType)]);
            }

            Entity entity = new Entity
            {
                Id = party.PartyUuid,
                Name = party.DisplayName,
                TypeId = entityType.Id,
                VariantId = entityVariant.Id,
                RefId = party.PartyUuid.ToString()
            };

            dbContext.Entities.Add(entity);
            var res = await dbContext.SaveChangesAsync(cancellationToken);

            if (res > 0)
            {
                result.PartyCreated = true;
            }
        }

        return result;
    }
}
