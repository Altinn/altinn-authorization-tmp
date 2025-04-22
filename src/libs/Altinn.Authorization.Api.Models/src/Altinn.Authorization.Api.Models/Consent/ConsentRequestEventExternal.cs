using Altinn.Authorization.Core.Models.Consent;
using Altinn.Authorization.Core.Models.Register;

namespace Altinn.Authorization.Api.Models.Consent
{
    /// <summary>
    /// represents an event related to a consent request.
    /// </summary>
    public class ConsentRequestEventExternal
    {
        /// <summary>
        /// The ID of the consent event that this event is related to.
        /// </summary>
        public Guid ConsentEventID { get; set; }

        /// <summary>
        /// When the event was created.
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// Who made the event happen
        /// </summary>
        public required ConsentPartyUrnExternal PerformedBy { get; set; }

        /// <summary>
        /// The type of event that happened.
        /// </summary>
        public ConsentRequestEventTypeExternal EventType { get; set; }

        /// <summary>
        /// The ID of the consent request that this event is related to.
        /// </summary>
        public Guid ConsentRequestID { get; set; }

        public static ConsentRequestEventExternal FromCore(ConsentRequestEvent core)
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
