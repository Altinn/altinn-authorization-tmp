using Altinn.AccessManagement.Api.Enterprise.Extensions;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Api.Models.Register;

namespace Altinn.AccessManagement.Api.Enterprise.Utils
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
            return new ConsentRequest
            {
                Id = consentRequestExternal.Id,
                From = ToCore(consentRequestExternal.From),
                To = ToCore(consentRequestExternal.To),
                RequiredDelegator = consentRequestExternal.RequiredDelegator != null
                    ? ToCore(consentRequestExternal.RequiredDelegator)
                    : null,
                ValidTo = consentRequestExternal.ValidTo,
                ConsentRights = consentRequestExternal.ConsentRights.Select(static x => x.ToConsentRightExternal()).ToList(),
                RequestMessage = consentRequestExternal.RequestMessage,
                RedirectUrl = consentRequestExternal.RedirectUrl
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
