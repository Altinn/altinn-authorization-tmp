using Altinn.Authorization.Core.Models.Party;

namespace Altinn.AccessManagement.Core.Repositories.Interfaces
{
    /// <summary>
    /// Interface for party repository
    /// </summary>
    public interface IAmPartyRepository
    {
        /// <summary>
        /// Get party by party uuid
        /// </summary>
        public Task<MinimalParty> GetByUuid(Guid partyUuid);

        /// <summary>
        /// Get by party id
        /// </summary>
        public Task<MinimalParty> GetByPartyId(int partyId);

        /// <summary>
        /// Get Party by org number
        /// </summary>
        public Task<MinimalParty> GetByOrgNo(string orgNo);

        /// <summary>
        /// Get Party by person number
        /// </summary>
        public Task<MinimalParty> GetByPersonNo(string personNo);
    }
}
