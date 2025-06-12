using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.Authorization.Api.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.AccessManagement.Api.Enterprise.Extensions
{
    /// <summary>
    /// Provides extension methods for transforming ConsentRequestDetails to ConsentRequestDetailsBFF.
    /// </summary>
    public static class ConsentRequestEventExtensions   
    {
        /// <summary>
        /// Converts a ConsentRequestEvent object to a ConsentRequestEventExternal object.
        /// </summary>
        /// <param name="core">The ConsentRequestEvent object to convert.</param>
        /// <returns>A ConsentRequestDetailsBFF object.</returns>
        /// <exception cref="ArgumentException">Thrown when an unknown consent party URN is encountered.</exception>
        public static ConsentRequestEventExternal ToConsentRequestEventExternal(this ConsentRequestEvent core)
        {
            ConsentPartyUrnExternal toExternal;

            if (core.PerformedBy.IsOrganizationId(out OrganizationNumber? organizationNumberTo))
            {
                toExternal = ConsentPartyUrnExternal.OrganizationId.Create(organizationNumberTo);
            }
            else if (core.PerformedBy.IsPersonId(out PersonIdentifier? personIdentifier))
            {
                toExternal = ConsentPartyUrnExternal.PersonId.Create(personIdentifier);
            }
            else if (core.PerformedBy.IsPartyUuid(out Guid partyUuid))
            {
                toExternal = ConsentPartyUrnExternal.PartyUuid.Create(partyUuid);
            }
            else
            {
                throw new ArgumentException("Unknown consent party urn");
            }

            return new ConsentRequestEventExternal
            {
                ConsentEventID = core.ConsentEventID,
                Created = core.Created,
                PerformedBy = toExternal,
                EventType = (ConsentRequestEventTypeExternal)core.EventType,
                ConsentRequestID = core.ConsentRequestID
            };
        }
    }
}
