namespace Altinn.AccessManagement.Api.Maskinporten.Models.Concent
{
    /// <summary>
    /// Represents the consent information for Maskinporten.
    /// </summary>
    public class ConsentInfoMaskinporten
    {
        /// <summary>
        /// The unique identifier for the consent. Same ID as concent request.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ConsentPartyUrn From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public ConsentPartyUrn To { get; set; }

        /// <summary>
        /// Defines when the consent was given.
        /// </summary>
        public DateTimeOffset Concented { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRightExternal> ConcentRights { get; set; }
    }
}
