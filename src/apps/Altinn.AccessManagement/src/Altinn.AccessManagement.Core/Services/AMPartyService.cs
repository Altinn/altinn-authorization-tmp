using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Interfaces;
using Altinn.Authorization.Core.Models.Party;

namespace Altinn.AccessManagement.Core.Services
{
    /// <summary>
    /// Default constructor
    /// </summary>
    public class AMPartyService(IAmPartyRepository ampartyRepository) : IAMPartyService
    {
        private readonly IAmPartyRepository _amPartyRepository = ampartyRepository;

        public Task<MinimalParty> GetByOrgNo(string orgNo)
        {
            throw new NotImplementedException();
        }

        public Task<MinimalParty> GetByPartyId(int partyId)
        {
            throw new NotImplementedException();
        }

        public Task<MinimalParty> GetByPersonNo(string personNo)
        {
            throw new NotImplementedException();
        }

        public Task<MinimalParty> GetByUid(Guid partyUuid)
        {
            throw new NotImplementedException();
        }
    }
}
