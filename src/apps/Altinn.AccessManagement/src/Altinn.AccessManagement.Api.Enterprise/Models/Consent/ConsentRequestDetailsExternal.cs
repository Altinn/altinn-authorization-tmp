using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Register.Core.Parties;

namespace Altinn.AccessManagement.Api.Enterprise.Models.Consent
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
        public required ConsentPartyUrnExternal2 From { get; set; }

        /// <summary>
        /// Defines the party requesting consent.
        /// </summary>
        public required ConsentPartyUrnExternal2 To { get; set; }

        /// <summary>
        /// Defines how long the concent is valid
        /// </summary>
        public required DateTimeOffset ValidTo { get; set; }

        /// <summary>
        /// The consented rights.
        /// </summary>
        public required List<ConsentRightExternal2> ConsentRights { get; set; }

        /// <summary>
        /// The request message
        /// </summary>
        public Dictionary<string, string>? Requestmessage { get; set; }

        /// <summary>
        /// Defines when the consent was given.
        /// </summary>
        public DateTimeOffset? Consented { get; set; }

        public static ConsentRequestDetailsExternal FromCore(ConsentRequestDetails core)
        {
            ConsentPartyUrnExternal2 to = ConsentPartyUrnExternal2.OrganizationId.Create(OrganizationNumber.Parse(core.To.ValueSpan));

            ConsentPartyUrnExternal2 from = core.From switch
            {
                ConsentPartyUrn.PersonId => ConsentPartyUrnExternal2.PersonId.Create(PersonIdentifier.Parse(core.From.ValueSpan)),
                ConsentPartyUrn.OrganizationId => ConsentPartyUrnExternal2.OrganizationId.Create(OrganizationNumber.Parse(core.From.ValueSpan)),
                _ => throw new ArgumentException("Unknown consent party urn")
            };

            return new ConsentRequestDetailsExternal
            {
                Id = core.Id,
                From = from,
                To = to,
                Consented = core.Consented,
                ValidTo = core.ValidTo,
                ConsentRights = core.ConsentRights.Select(ConsentRightExternal2.FromCore).ToList()
            };
        }

    }
}
