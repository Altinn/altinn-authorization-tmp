using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;

namespace Altinn.AccessManagement.Api.Internal.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentRequestDetails to ConsentRequestDetailsBFF.
    /// </summary>
    public static class ConsentRequestDetailsExtensions
    {
        /// <summary>
        /// Converts a ConsentRequestDetails object to a ConsentRequestDetailsBFF object.
        /// </summary>
        /// <param name="details">The ConsentRequestDetails object to convert.</param>
        /// <returns>A ConsentRequestDetailsBFF object.</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown consent party URN is encountered.</exception>
        public static ConsentRequestDetailsBFF ToConsentRequestDetailsBFF(this ConsentRequestDetails details)
        {
            ConsentPartyUrnExternal to;
            if (details.To.IsPartyUuid(out Guid toPartyUuid))
            {
                to = ConsentPartyUrnExternal.PartyUuid.Create(toPartyUuid);
            }
            else
            {
                throw new ArgumentException("Unknown consent party urn");
            }

            ConsentPartyUrnExternal from;
            if (details.From.IsPartyUuid(out Guid fromPartyUuid))
            {
                from = ConsentPartyUrnExternal.PartyUuid.Create(fromPartyUuid);
            }
            else
            {
                throw new ArgumentException("Unknown consent party urn");
            }

            ConsentPartyUrnExternal? requiredDelegator = null;
            if (details.RequiredDelegator != null && details.RequiredDelegator.IsPartyUuid(out Guid delegatorUuid))
            {
                requiredDelegator = ConsentPartyUrnExternal.PartyUuid.Create(delegatorUuid);
            }

            ConsentPartyUrnExternal? handledBy = null;
            if (details.HandledBy != null && details.HandledBy.IsPartyUuid(out Guid handledByUuid))
            {
                handledBy = ConsentPartyUrnExternal.PartyUuid.Create(handledByUuid);
            }

            return new ConsentRequestDetailsBFF
            {
                Id = details.Id,
                From = from,
                To = to,
                RequiredDelegator = requiredDelegator,
                HandledBy = handledBy,
                Consented = details.Consented,
                ValidTo = details.ValidTo,
                ConsentRights = details.ConsentRights.Select(ConsentRightExternal.FromCore).ToList(),
                ConsentRequestEvents = details.ConsentRequestEvents.Select(ConsentRequestEventExternal.FromCore).ToList(),
                RedirectUrl = details.RedirectUrl,
                ViewUri = details.ViewUri,
                TemplateId = details.TemplateId,
                TemplateVersion = details.TemplateVersion,
                Requestmessage = details.RequestMessage
            };
        }
    }
}
