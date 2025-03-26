using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Repositories;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.Core.Models.Party;

namespace Altinn.AccessMgmt.Persistence.Services
{
    public class AMPartyService(IEntityRepository entityRepository, IEntityLookupRepository entityLookupRepository) : IAmPartyRepository
    {
        private readonly IEntityRepository entityRepository = entityRepository;
        private readonly IEntityLookupRepository entityLookupRepository = entityLookupRepository;

        public async Task<MinimalParty> GetByOrgNo(string orgNo)
        {
            GenericFilterBuilder<AccessMgmt.Core.Models.EntityLookup> filter = entityLookupRepository.CreateFilterBuilder();
            filter.Add(t => t.Key, "OrganizationIdentifier", Core.Helpers.FilterComparer.Contains);
            filter.Equal(t => t.Value, orgNo);

            IEnumerable<AccessMgmt.Core.Models.ExtEntityLookup> res = await entityLookupRepository.GetExtended(filter);

            if (res == null || !res.Any())
            {
                return null;
            }

            if (res.Count() > 1)
            {
                throw new Exception("Multiple matches");
            }

            ExtEntityLookup extEntityLookup = res.First();

            return new MinimalParty()
            {
                Name = extEntityLookup.Entity.Name,
                PartyUuid = extEntityLookup.Entity.Id,
                OrgNo = extEntityLookup.Value,
            };
        }

        public Task<MinimalParty> GetByPartyId(int partyId)
        {
            throw new NotImplementedException();
        }

        public async Task<MinimalParty> GetByPersonNo(string personNo)
        {
            GenericFilterBuilder<AccessMgmt.Core.Models.EntityLookup> filter = entityLookupRepository.CreateFilterBuilder();
            filter.Add(t => t.Key, "PersonIdentifier", Core.Helpers.FilterComparer.Contains);
            filter.Equal(t => t.Value, personNo);

            IEnumerable<AccessMgmt.Core.Models.ExtEntityLookup> res = await entityLookupRepository.GetExtended(filter);

            if (res == null || !res.Any())
            {
                return null;
            }

            if (res.Count() > 1)
            {
                throw new Exception("Multiple matches");
            }

            ExtEntityLookup extEntityLookup = res.First();

            return new MinimalParty()
            {
                Name = extEntityLookup.Entity.Name,
                PartyUuid = extEntityLookup.Entity.Id,
                OrgNo = extEntityLookup.Value,
            };
        }

        public Task<MinimalParty> GetByUid(Guid partyUuid)
        {
            throw new NotImplementedException();
        }
    }
}
