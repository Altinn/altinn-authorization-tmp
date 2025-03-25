namespace Altinn.Authorization.Core.Models.Party
{
    public class MinimalParty
    {
        /// <summary>
        /// The partyUuid of the party
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// The partyId of the party
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// The name of the party
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///  The organization number of the party
        /// </summary>
        public string? OrgNo { get; set; }

        /// <summary>
        /// The person number of the party
        /// </summary>
        public string? PersonNo { get; set; }
    }
}
