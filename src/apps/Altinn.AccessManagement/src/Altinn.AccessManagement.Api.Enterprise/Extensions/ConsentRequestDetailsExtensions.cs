using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.AccessManagement.Api.Enterprise.Extensions
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
        public static ConsentRequestDetailsExternal ToConsentRequestDetailsExternal(this ConsentRequestDetails details)
        {
            ConsentPartyUrnExternal to = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(details.To.ValueSpan));

            ConsentPartyUrnExternal from = details.From switch
            {
                ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(details.From.ValueSpan)),
                ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(details.From.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return new ConsentRequestDetailsExternal
            {
                Id = details.Id,
                From = from,
                To = to,
                RequiredDelegator = details.RequiredDelegator != null
                    ? details.RequiredDelegator switch
                    {
                        ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(details.RequiredDelegator.ValueSpan)),
                        ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(details.RequiredDelegator.ValueSpan)),
                        _ => throw new ArgumentException("Unknown consent party urn")
                    }
                    : null,
                HandledBy = details.HandledBy != null
                    ? details.HandledBy switch
                    {
                        ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(details.HandledBy.ValueSpan)),
                        ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(details.HandledBy.ValueSpan)),
                        _ => throw new ArgumentException("Unknown consent party urn")
                    }
                    : null,
                Consented = details.Consented,
                ValidTo = details.ValidTo,
                ConsentRights = [.. details.ConsentRights.Select(static x => x.ToConsentRightExternal())],
                ConsentRequestEvents = [.. details.ConsentRequestEvents.Select(static x => x.ToConsentRequestEventExternal())],
                RedirectUrl = details.RedirectUrl,
                ViewUri = details.ViewUri,
                RequestMessage = details.RequestMessage != null
                    ? new Dictionary<string, string>(details.RequestMessage)
                    : null
            };
        }
    }
}
