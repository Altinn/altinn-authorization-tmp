namespace Altinn.Authorization.Core.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequest
    {
        /// <summary>
        /// Defines the ID for the consent request.Created by Altinn
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Defines the party that has to consent to the consentRequest
        /// </summary>
        public required ConsentPartyUrn From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrn To { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public required DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRight> ConsentRights { get; set; }

        /// <summary>
        /// The request message
        /// </summary>
        public Dictionary<string, string> Requestmessage { get; set; }

    }
}
