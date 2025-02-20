using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.Register.Core.Parties;
using Microsoft.IdentityModel.Abstractions;

namespace Altinn.AccessManagement.Api.Enterprise.Models.Consent
{
    /// <summary>
    /// Represents a consent request.
    /// </summary>
    public class ConsentRequestExternal
    {
        /// <summary>
        /// Defines the party to request consent from.
        /// </summary>
        public required ConsentPartyUrnExternal From { get; set; }

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
        /// Maps from external consent request to internal consent request
        /// </summary>
        /// <returns></returns>
        public ConsentRequest ToCore()
        {
            // The id 
            Guid consentId = Guid.NewGuid();

            ConsentPartyUrn fromInternal = null;
            if (From.IsOrganizationId(out OrganizationNumber? organizationNumber))
            {
                fromInternal = ConsentPartyUrn.OrganizationId.Create(organizationNumber);
            }
            else if (From.IsPersonId(out PersonIdentifier? personIdentifier))
            {
                fromInternal = ConsentPartyUrn.PersonId.Create(personIdentifier);
            }

            ConsentPartyUrn toInternal = null;
            if (To.IsOrganizationId(out OrganizationNumber? organizationNumberTo))
            {
                toInternal = ConsentPartyUrn.OrganizationId.Create(organizationNumberTo);
            }
            else if (To.IsPersonId(out PersonIdentifier? personIdentifier))
            {
                toInternal = ConsentPartyUrn.PersonId.Create(personIdentifier);
            }

            return new ConsentRequest
            {
                Id = consentId,
                From = fromInternal,
                To = toInternal,
                ValidTo = ValidTo,
                ConsentRights = ConsentRights.Select(ConsentRightExternal.ToCore).ToList(),
                Requestmessage = Requestmessage
            };
        }
    }
}
