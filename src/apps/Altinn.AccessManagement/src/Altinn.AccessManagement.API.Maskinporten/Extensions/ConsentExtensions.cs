using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Api.Models.Register;

namespace Altinn.AccessManagement.Api.Maskinporten.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentRequestDetails to ConsentRequestDetailsBFF.
    /// </summary>
    public static class ConsentExtensions
    {
        /// <summary>
        /// Converts a Consent object to a Consent object.
        /// </summary>
        /// <param name="consent">The Consent object to convert.</param>
        /// <returns>A ConsentRequestDetailsBFF object.</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown consent party URN is encountered.</exception>
        public static ConsentInfoMaskinporten ToConsentInfoMaskinporten(this Consent consent)
        {
            ConsentPartyUrnExternal to = ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(consent.To.ValueSpan));

            ConsentPartyUrnExternal from = consent.From switch
            {
                ConsentPartyUrn.PersonId => ConsentPartyUrnExternal.PersonId.Create(PersonIdentifier.Parse(consent.From.ValueSpan)),
                ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal.OrganizationId.Create(OrganizationNumber.Parse(consent.From.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return new ConsentInfoMaskinporten
            {
                Id = consent.Id,
                From = from,
                To = to,
                Consented = consent.Consented,
                ValidTo = consent.ValidTo,
                ConsentRights = [.. consent.ConsentRights.Select(static x => x.ToConsentRightExternal())]
            };
        }
    }
}
