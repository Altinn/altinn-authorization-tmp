using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequestDetailsExternal
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
        public Dictionary<string, string>? RequestMessage { get; set; }

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

        public static ConsentRequestDetailsExternal FromCore(ConsentRequestDetails core)
        {
            ConsentPartyUrnExternal to = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(core.To.ValueSpan));

            ConsentPartyUrnExternal from = core.From switch
            {
                ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(core.From.ValueSpan)),
                ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(core.From.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return new ConsentRequestDetailsExternal
            {
                Id = core.Id,
                From = from,
                To = to,
                RequiredDelegator = core.RequiredDelegator != null
                    ? core.RequiredDelegator switch
                    {
                        ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(core.RequiredDelegator.ValueSpan)),
                        ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(core.RequiredDelegator.ValueSpan)),
                        _ => throw new ArgumentException("Unknown consent party urn")
                    }
                    : null,
                HandledBy = core.HandledBy != null
                    ? core.HandledBy switch
                    {
                        ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(core.HandledBy.ValueSpan)),
                        ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(core.HandledBy.ValueSpan)),
                        _ => throw new ArgumentException("Unknown consent party urn")
                    }
                    : null,
                Consented = core.Consented,
                ValidTo = core.ValidTo,
                ConsentRights = core.ConsentRights.Select(ConsentRightExternal.FromCore).ToList(),
                ConsentRequestEvents = core.ConsentRequestEvents.Select(ConsentRequestEventExternal.FromCore).ToList(),
                RedirectUrl = core.RedirectUrl,
                ViewUri = core.ViewUri,
                RequestMessage = core.RequestMessage != null
                    ? new Dictionary<string, string>(core.RequestMessage)
                    : null
            };
        }
    }
}
