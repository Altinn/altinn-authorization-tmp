using Altinn.Authorization.Core.Models.Consent;

namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequestExternal
    {
        /// <summary>
        /// The unique identifier for the consent request. Need to be unique across all consent requests.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Defines the party to request consent from.
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

        /// <summary>
        /// Defines the party to request consent from.
        /// </summary>
        public ConsentPartyUrnExternal? RequiredDelegator { get; set; }

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

        /// <summary>
        /// Redirect url for the user to be redirected after consent is given or denied.
        /// </summary>
        public required string RedirectUrl { get; set; } = string.Empty;
    }
}
