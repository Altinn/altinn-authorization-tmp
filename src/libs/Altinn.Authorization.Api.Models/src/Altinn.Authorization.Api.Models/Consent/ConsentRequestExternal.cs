namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequestExternal
    {
        /// <summary>
        /// Defines the party to request consent from.
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrnExternal To { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public required DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRightExternal> ConsentRights { get; set; }

        /// <summary>
        /// The request message
        /// </summary>
        public Dictionary<string, string>? Requestmessage { get; set; }
    }
}
