using System.Data;
using Altinn.AccessManagement.Core.Enums.Consent;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.Register.Core.Parties;
using Npgsql;

namespace Altinn.AccessManagement.Persistence.Consent
{
    /// <summary>
    /// Repository for handling consent data.
    /// </summary>
    public class ConsentRepository : IConsentRepository
    {
        private readonly NpgsqlDataSource _conn;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRepository"/> class
        /// </summary>
        public ConsentRepository(NpgsqlDataSource conn)
        {
            _conn = conn;
        }

        /// <inheritdoc/>
        public Task ApproveConsentRequest(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task DeleteRequest(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<Core.Models.Consent.Consent>> GetAllConsents(Guid partyUid)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<Core.Models.Consent.Consent> GetConsent(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> GetRequest(Guid consentRequestId)
        {
            List<ConsentRight> consentRight = await GetConsentRights(consentRequestId);

            string consentQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentrequest 
                WHERE id = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentQuery);
            pgcom.Parameters.AddWithValue("id", consentRequestId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string from = reader.GetFieldValue<string>("from");
                string to = reader.GetFieldValue<string>("to");

                ConsentPartyUrn fromPartyUrn = null;
                ConsentPartyUrn toPartyUrn = null;

                if (PersonIdentifier.TryParse(from, provider: null, out PersonIdentifier personIdentifier))
                {
                    fromPartyUrn = ConsentPartyUrn.PersonId.Create(personIdentifier);
                }
                else if (OrganizationNumber.TryParse(from, provider: null, out OrganizationNumber organizationIdentifier))
                {
                    fromPartyUrn = ConsentPartyUrn.OrganizationId.Create(organizationIdentifier);
                }

                if (PersonIdentifier.TryParse(to, provider: null, out PersonIdentifier personIdentifierTo))
                {
                    toPartyUrn = ConsentPartyUrn.PersonId.Create(personIdentifierTo);
                }
                else if (OrganizationNumber.TryParse(to, provider: null, out OrganizationNumber organizationIdentifierTo))
                {
                    toPartyUrn = ConsentPartyUrn.OrganizationId.Create(organizationIdentifierTo);
                }

                if (fromPartyUrn == null || toPartyUrn == null)
                {
                    throw new InvalidDataException("Invalid party URN");
                }

                return new ConsentRequestDetails
                {
                    Id = consentRequestId,
                    From = fromPartyUrn,
                    To = toPartyUrn,
                    ValidTo = reader.GetFieldValue<DateTimeOffset>("validTo"),
                    ConsentRights = consentRight,
                    Requestmessage = reader.GetFieldValue<Dictionary<string, string>>("requestMessage"),
                    ConsentRequestStatus = reader.GetFieldValue<ConsentRequestStatusType>("status"),
                    Consented = reader.GetFieldValue<DateTimeOffset?>("consented")
                };
            }

            return null;
        }

        /// <inheritdoc/>
        public Task RejectConsentRequest(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task Revoke(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Return the consent rights
        /// </summary>
        private async Task<List<ConsentRight>> GetConsentRights(Guid consentRequestId)
        {
            Dictionary<Guid, List<ConsentResourceAttribute>> keyValuePairs = await GetConsentResourceAttributes(consentRequestId);

            string consentRightsQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentright 
                WHERE concentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentRightsQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);
            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            List<ConsentRight> consentRights = new List<ConsentRight>();
            while (await reader.ReadAsync())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("concentRightId");

                List<ConsentResourceAttribute> resourceAttributes = new List<ConsentResourceAttribute>();
                if (keyValuePairs.TryGetValue(consentRightId, out List<ConsentResourceAttribute> foundAttributes))
                {
                    resourceAttributes = foundAttributes;
                }

                Dictionary<string, string> metadata = [];
                Dictionary<Guid, Dictionary<string, string>> consentMetadata = await GetConsentRightMetadata(consentRequestId);
                if (consentMetadata.TryGetValue(consentRightId, out Dictionary<string, string> foundMetadata))
                {
                    metadata = foundMetadata;
                }
                else
                {
                    metadata = null;
                }

                consentRights.Add(new ConsentRight
                {
                    Action = reader.GetFieldValue<List<string>>("action"),
                    Resource = resourceAttributes,
                    MetaData = metadata
                });
            }

            return consentRights;
        }

        /// <summary>
        ///  Gets the consent resource attributes for a consent request. Returned as a dictinary to be able to group the attributes by consent right.
        /// </summary>
        private async Task<Dictionary<Guid, List<ConsentResourceAttribute>>> GetConsentResourceAttributes(Guid consentRequestId)
        {
            string consentResourcesQuery = /*strpsql*/@$"
                SELECT * FROM consent.resourceattributes ra 
                join consent.concentright cr on cr.consentRightId = ra.consentRightId 
                WHERE cr.concentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentResourcesQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            Dictionary<Guid, List<ConsentResourceAttribute>> keyValuePairs = new Dictionary<Guid, List<ConsentResourceAttribute>>();

            while (reader.Read())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("concentRightId");
                ConsentResourceAttribute consentResourceAttribute = new ConsentResourceAttribute
                {
                    Type = reader.GetFieldValue<string>("type"),
                    Value = reader.GetFieldValue<string>("value")
                };
                if (keyValuePairs.ContainsKey(consentRightId))
                {
                    keyValuePairs[consentRightId].Add(consentResourceAttribute);
                }
                else
                {
                    keyValuePairs.Add(consentRightId, new List<ConsentResourceAttribute> { consentResourceAttribute });
                }
            }

            return keyValuePairs;
        }

        private async Task<Dictionary<Guid, Dictionary<string, string>>> GetConsentRightMetadata(Guid consentRequestId)
        {
            string consentMetadataQuery = /*strpsql*/@$"
                SELECT * FROM consent.metadata ra 
                join consent.consentright cr on cr.consentRightId = ra.consentRightId
                WHERE cr.concentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentMetadataQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);
            Dictionary<Guid, Dictionary<string, string>> consentMetadata = [];

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            while (reader.Read())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("concentRightId");
                string id = reader.GetFieldValue<string>("id");
                string value = reader.GetFieldValue<string>("value");

                if (consentMetadata.ContainsKey(consentRightId))
                {
                    consentMetadata[consentRightId].Add(id, value);
                }
                else
                {
                    consentMetadata.Add(consentRightId, new Dictionary<string, string> { { id, value } });
                }
            }

            return consentMetadata;
        }
    }
}
