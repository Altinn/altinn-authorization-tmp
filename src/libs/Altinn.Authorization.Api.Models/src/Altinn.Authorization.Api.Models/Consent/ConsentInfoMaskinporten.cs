namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Represents the consent information for Maskinporten.
    /// </summary>
    public class ConsentInfoMaskinporten
    {
        /// <summary>
        /// The unique identifier for the consent. Same ID as concent request.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Defines who is gives consent 
        /// </summary>
        public required ConsentPartyUrnExternal? From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrnExternal To { get; set; }

        /// <summary>
        /// Defines when the consent was given.
        /// </summary>
        public required DateTimeOffset Consented { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public required DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRightExternal> ConsentRights { get; set; }
    }
}
