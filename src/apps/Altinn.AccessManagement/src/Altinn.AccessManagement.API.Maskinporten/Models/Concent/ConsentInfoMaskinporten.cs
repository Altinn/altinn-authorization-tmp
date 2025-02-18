using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Register.Core.Parties;

namespace Altinn.AccessManagement.Api.Maskinporten.Models.Concent
{
    /// <summary>
    /// Represents the consent information for Maskinporten.
    /// </summary>
    public class ConsentInfoMaskinporten
    {
        /// <summary>
        /// The unique identifier for the consent. Same ID as concent request.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrnExternal To { get; set; }

        /// <summary>
        /// Defines when the consent was given.
        /// </summary>
        public DateTimeOffset Concented { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRightExternal> ConcentRights { get; set; }

        /// <summary>
        /// Maps from internal consent to external consent
        /// </summary>
        public static ConsentInfoMaskinporten Convert(Consent consent)
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
                Concented = consent.Concented,
                ValidTo = consent.ValidTo,
                ConcentRights = consent.ConcentRights.Select(ConsentRightExternal.FromCore).ToList()
            };
        }
    }
}
