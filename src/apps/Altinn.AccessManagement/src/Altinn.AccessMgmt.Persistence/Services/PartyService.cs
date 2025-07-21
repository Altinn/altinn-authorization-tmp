using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Party;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Contracts;
using Altinn.AccessMgmt.Persistence.Services.Models;
using Altinn.Authorization.ProblemDetails;

namespace Altinn.AccessMgmt.Persistence.Services
{
    public class PartyService(
        IEntityRepository entityRepository,
        IEntityTypeRepository entityTypeRepository,
        IEntityVariantRepository entityVariantRepository,
        IEntityLookupRepository entityLookupRepository
        ) : IPartyService
    {
        private readonly IEntityRepository _entityRepository = entityRepository;
        private readonly IEntityTypeRepository _entityTypeRepository = entityTypeRepository;
        private readonly IEntityVariantRepository _entityVariantRepository = entityVariantRepository;
        private readonly IEntityLookupRepository _entityLookupRepository = entityLookupRepository;

        public async Task<Result<AddPartyResult>> AddParty(PartyBaseInternal party, ChangeRequestOptions options, CancellationToken cancellationToken = default)
        {
            AddPartyResult result = new() { PartyUuid = party.PartyUuid, PartyCreated = false };

            var entityExist = await _entityRepository.Get(e => e.Id, party.PartyUuid, cancellationToken: cancellationToken);

            if (!entityExist.Any())
            {
                /*
                 * TODO: For now this is restricted to SystemUser as it does not have any logic to set the correct RefId based on entity type Organisation number for Organisation and Fødselsnummer for Person. 
                 * Also there should be a matching record in the EntityLookup table for required lookup values SystemUSer does not have any lookup values as the only id it has is its PartyUuid.
                 */
                if (!party.EntityType.Equals("Systembruker", StringComparison.InvariantCultureIgnoreCase))
                {
                    return Problems.UnsuportedEntityType.Create([new("entityType", party.EntityType.ToString())]);
                }

                var entityTypeId = (await _entityTypeRepository.Get(t => t.Name, party.EntityType)).FirstOrDefault()?.Id;
                if (entityTypeId == null)
                {
                    return Problems.EntityTypeNotFound.Create([new("entityType", party.EntityType)]);
                }

                var entityVariantId = (await _entityVariantRepository.Get(t => t.TypeId, entityTypeId)).FirstOrDefault(t => t.Name.Equals(party.EntityVariantType, StringComparison.OrdinalIgnoreCase))?.Id;
                if (entityVariantId == null)
                {
                    return Problems.EntityVariantNotFound.Create([new("entityVariantId", party.EntityVariantType)]);
                }

                Entity entity = new Entity
                {
                    Id = party.PartyUuid,
                    Name = party.DisplayName,
                    TypeId = entityTypeId.Value,
                    VariantId = entityVariantId.Value,
                    RefId = party.PartyUuid.ToString(),
                    ParentId = party.ParentPartyUuid
                };

                var res = await _entityRepository.Create(entity, options, cancellationToken);

                if (res > 0)
                {
                    result.PartyCreated = true;
                }                
            }
            
            return result;
        }
    }
}
