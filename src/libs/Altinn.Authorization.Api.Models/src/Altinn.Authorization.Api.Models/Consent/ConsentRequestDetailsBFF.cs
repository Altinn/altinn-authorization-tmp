using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequestDetailsBFF
    {
        /// <summary>
        /// The id of the consent request.
        /// </summary>
        public required Guid Id { get; set; }

        /// <summary>
        /// Defines the party to request consent from.
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrnExternal To { get; set; }

        /// <summary>
        /// The required party that need to accept the consent request.
        /// </summary>
        public ConsentPartyUrnExternal? RequiredDelegator { get; set; }

        /// <summary>
        /// The handled by party that handles the consent request on behalf of the requesting party.
        /// </summary>
        public ConsentPartyUrnExternal? HandledBy { get; set; }

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
        /// Defines when the consent was given.
        /// </summary>
        public DateTimeOffset? Consented { get; set; }

        /// <summary>
        /// Redirect url for the user to be redirected after consent is given or denied.
        /// </summary>
        public required string RedirectUrl { get; set; } = string.Empty;

        /// <summary>
        /// List all events related to consent request
        /// </summary>
        public required List<ConsentRequestEventExternal> ConsentRequestEvents { get; set; }

        /// <summary>
        /// The URI for the view that should be shown to the user when requesting consent.
        /// </summary>
        public string ViewUri { get; set; } = string.Empty;

        /// <summary>
        /// The consent template id.
        /// </summary>
        public string? TemplateId { get; set; }

        /// <summary>
        /// The version of the consent template.
        /// </summary>
        public int? TemplateVersion { get; set; }

        public static ConsentRequestDetailsBFF FromCore(ConsentRequestDetails core)
        {
            ConsentPartyUrnExternal to;
            if (core.To.IsPartyUuid(out Guid toPartyUuid))
            {
                to = ConsentPartyUrnExternal.PartyUuid.Create(toPartyUuid);
            }
            else
            {
                throw new ArgumentException("Unknown consent party urn");
            }

            ConsentPartyUrnExternal from;
            if (core.From.IsPartyUuid(out Guid fromPartyUuid))
            {
                from = ConsentPartyUrnExternal.PartyUuid.Create(fromPartyUuid);
            }
            else
            {
                throw new ArgumentException("Unknown consent party urn");
            }

            ConsentPartyUrnExternal? requiredDelegator = null;
            if (core.RequiredDelegator != null && core.RequiredDelegator.IsPartyUuid(out Guid delegatorUuid))
            {
                requiredDelegator = ConsentPartyUrnExternal.PartyUuid.Create(delegatorUuid);
            }

            ConsentPartyUrnExternal? handledBy = null;
            if (core.HandledBy != null && core.HandledBy.IsPartyUuid(out Guid handledByUuid))
            {
                handledBy = ConsentPartyUrnExternal.PartyUuid.Create(handledByUuid);
            }

            return new ConsentRequestDetailsBFF
            {
                Id = core.Id,
                From = from,
                To = to,
                RequiredDelegator = requiredDelegator,
                HandledBy = handledBy,
                Consented = core.Consented,
                ValidTo = core.ValidTo,
                ConsentRights = core.ConsentRights.Select(ConsentRightExternal.FromCore).ToList(),
                ConsentRequestEvents = core.ConsentRequestEvents.Select(ConsentRequestEventExternal.FromCore).ToList(),
                RedirectUrl = core.RedirectUrl,
                ViewUri = core.ViewUri,
                TemplateId = core.TemplateId,
                TemplateVersion = core.TemplateVersion,
                Requestmessage = core.Requestmessage
            };
        }
    }
}
