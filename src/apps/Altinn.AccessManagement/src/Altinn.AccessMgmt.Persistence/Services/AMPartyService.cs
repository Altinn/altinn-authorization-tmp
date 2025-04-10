using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.Core.Models.Party;

namespace Altinn.AccessMgmt.Persistence.Services
{
    /// <summary>
    /// Repository service to lookupp party information
    /// </summary>
    public class AMPartyService(IEntityRepository entityRepository, IEntityLookupRepository entityLookupRepository) : IAmPartyRepository
    {
        private readonly IEntityRepository entityRepository = entityRepository;
        private readonly IEntityLookupRepository entityLookupRepository = entityLookupRepository;

        /// <inheritdoc />
        public async Task<MinimalParty> GetByOrgNo(string orgNo, CancellationToken cancellationToken = default)
        {
            GenericFilterBuilder<AccessMgmt.Core.Models.EntityLookup> filter = entityLookupRepository.CreateFilterBuilder();
            filter.Add(t => t.Key, "OrganizationIdentifier", Core.Helpers.FilterComparer.Contains);
            filter.Equal(t => t.Value, orgNo);

            IEnumerable<AccessMgmt.Core.Models.ExtEntityLookup> res = await entityLookupRepository.GetExtended(filter, cancellationToken: cancellationToken);

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
                OrganizationId = extEntityLookup.Value,
            };
        }

        /// <inheritdoc />
        public Task<MinimalParty> GetByPartyId(int partyId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<MinimalParty> GetByPersonNo(string personNo, CancellationToken cancellationToken = default)
        {
            GenericFilterBuilder<AccessMgmt.Core.Models.EntityLookup> filter = entityLookupRepository.CreateFilterBuilder();
            filter.Add(t => t.Key, "PersonIdentifier", Core.Helpers.FilterComparer.Contains);
            filter.Equal(t => t.Value, personNo);

            IEnumerable<AccessMgmt.Core.Models.ExtEntityLookup> res = await entityLookupRepository.GetExtended(filter, cancellationToken: cancellationToken);

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
                OrganizationId = extEntityLookup.Value,
            };
        }

        /// <inheritdoc />
        public async Task<MinimalParty> GetByUuid(Guid partyUuid, CancellationToken cancellationToken = default)
        {
            IEnumerable<ExtEntityLookup> parties = await entityLookupRepository.GetExtended(t => t.EntityId, partyUuid, cancellationToken: cancellationToken);
            var res = parties.ToDictionary(t => t.Key, t => t.Value);

            if (res == null || !res.Any())
            {
                return null;
            }

            MinimalParty party = new MinimalParty();
            party.PartyUuid = partyUuid;
            party.Name = parties.First().Entity.Name;

            if (res.TryGetValue("OrganizationIdentifier", out string orgNo))
            {
                party.OrganizationId = orgNo;
            }

            if (res.TryGetValue("PersonIdentifier", out string personNo))
            {
                party.PersonId = personNo;
            }

            if (res.TryGetValue("Name", out string name))
            {
                party.Name = name;
            }

            return party;
        }
    }
}
