#nullable enable
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.AccessManagement.Api.Enduser.Utils
{
    /// <summary>
    /// Mapper for converting between external and internal models
    /// </summary>
    public static class ModelMapper
    {
        /// <summary>
        /// Converts external consent request to internal model
        /// </summary>
        public static ConsentRequest ToCore(ConsentRequestExternal consentRequestExternal)
        {
            // The id 
            Guid consentId = Guid.NewGuid();

            return new ConsentRequest
            {
                Id = consentId,
                From = ToCore(consentRequestExternal.From),
                To = ToCore(consentRequestExternal.To),
                ValidTo = consentRequestExternal.ValidTo,
                ConsentRights = consentRequestExternal.ConsentRights.Select(ConsentRightExternal.ToCore).ToList(),
                Requestmessage = consentRequestExternal.Requestmessage
            };
        }

        private static ConsentPartyUrn ToCore(ConsentPartyUrnExternal consentPartyUrnExternal)
        {
            return consentPartyUrnExternal switch
            {
                _ when consentPartyUrnExternal.IsOrganizationId(out OrganizationNumber? organizationNumber) =>
                    ConsentPartyUrn.OrganizationId.Create(organizationNumber),
                _ when consentPartyUrnExternal.IsPersonId(out PersonIdentifier? personIdentifier) =>
                    ConsentPartyUrn.PersonId.Create(personIdentifier),
                _ when consentPartyUrnExternal.IsPartyUuid(out Guid partyUuid) =>
                    ConsentPartyUrn.PartyUuid.Create(partyUuid),
                _ => throw new ArgumentException("Unknown consent party urn")
            };
        }
    }
}
