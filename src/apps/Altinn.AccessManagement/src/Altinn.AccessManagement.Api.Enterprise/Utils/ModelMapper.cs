using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.AccessManagement.Api.Enterprise.Utils
{
    public static class ModelMapper
    {

        public static ConsentRequest ToCore(ConsentRequestExternal consentRequestExternal)
        {
            // The id 
            Guid consentId = Guid.NewGuid();

            ConsentPartyUrn fromInternal = null;
            if (consentRequestExternal.From.IsOrganizationId(out OrganizationNumber? organizationNumber))
            {
                fromInternal = ConsentPartyUrn.OrganizationId.Create(organizationNumber);
            }
            else if (consentRequestExternal.From.IsPersonId(out PersonIdentifier? personIdentifier))
            {
                fromInternal = ConsentPartyUrn.PersonId.Create(personIdentifier);
            }

            ConsentPartyUrn toInternal = null;
            if (consentRequestExternal.To.IsOrganizationId(out OrganizationNumber? organizationNumberTo))
            {
                toInternal = ConsentPartyUrn.OrganizationId.Create(organizationNumberTo);
            }
            else if (consentRequestExternal.To.IsPersonId(out PersonIdentifier? personIdentifier))
            {
                toInternal = ConsentPartyUrn.PersonId.Create(personIdentifier);
            }

            return new ConsentRequest
            {
                Id = consentId,
                From = fromInternal,
                To = toInternal,
                ValidTo = consentRequestExternal.ValidTo,
                ConsentRights = consentRequestExternal.ConsentRights.Select(ConsentRightExternal.ToCore).ToList(),
                Requestmessage = consentRequestExternal.Requestmessage
            };
        }
    }
}
