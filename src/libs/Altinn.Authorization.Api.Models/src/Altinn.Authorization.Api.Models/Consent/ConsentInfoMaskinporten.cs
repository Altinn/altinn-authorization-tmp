using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

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

        /// <summary>
        /// Maps from internal consent to external consent
        /// </summary>
        public static ConsentInfoMaskinporten Convert(Altinn.Authorization.Core.Models.Consent.Consent consent)
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
                ConsentRights = consent.ConsentRights.Select(ConsentRightExternal.FromCore).ToList()
            };
        }
    }
}
