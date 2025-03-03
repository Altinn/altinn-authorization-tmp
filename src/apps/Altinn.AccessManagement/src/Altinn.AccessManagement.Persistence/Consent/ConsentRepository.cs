using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Enums.Consent;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Enums;
using Altinn.Register.Core.Parties;
using Npgsql;
using NpgsqlTypes;

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
        public async Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest)
        {
            const string consentRquestQuery = /*strpsql*/@"
                INSERT INTO consent.consentrequest (consentRequestId, fromPartyUuid, toPartyUuid, validTo, requestMessage)
                VALUES (
                @consentRequestId, 
                @fromPartyUuid, 
                @toPartyUuid, 
                @validTo, 
                @requestMessage)
                RETURNING consentRequestId;
                ";
   
            await using NpgsqlCommand command = _conn.CreateCommand(consentRquestQuery);
            command.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid,  consentRequest.Id);
            if (consentRequest.From.IsPartyUuid(out Guid fromPartyGuid))
            {
                command.Parameters.AddWithValue("fromPartyUuid", NpgsqlDbType.Uuid, fromPartyGuid);
            }
            else
            {
                throw new InvalidDataException("Invalid party URN");
            }

            if (consentRequest.From.IsPartyUuid(out Guid toPartyGuid))
            {
                command.Parameters.AddWithValue("toPartyUuid", NpgsqlDbType.Uuid, toPartyGuid);
            }
            else
            {
                throw new InvalidDataException("Invalid party URN");
            }

            command.Parameters.AddWithValue("requestMessage", NpgsqlDbType.Hstore, consentRequest.Requestmessage);

            command.Parameters.AddWithValue("validTo", NpgsqlDbType.TimestampTz, consentRequest.ValidTo.ToOffset(TimeSpan.Zero));
            await command.ExecuteNonQueryAsync();

            foreach (ConsentRight consentRight in consentRequest.ConsentRights)
            {
                Guid consentRightGuid = Guid.NewGuid();

                const string rightsQuery = /*strpsql*/@"
                INSERT INTO consent.consentright(consentRightId , consentRequestId , action)
                VALUES (
                @consentRightId, 
                @consentRequestId, 
                @action
                )
                RETURNING consentRightId;
                ";

                await using NpgsqlCommand rightsCommand = _conn.CreateCommand(rightsQuery);
                rightsCommand.Parameters.AddWithValue("consentRightId", consentRightGuid);
                rightsCommand.Parameters.AddWithValue("consentRequestId", consentRequest.Id);
                rightsCommand.Parameters.AddWithValue("action", consentRight.Action);
                await rightsCommand.ExecuteNonQueryAsync();

                // Bulding up the query for the resource attributes. Typical this is only one, but in theory it can be multiple attributes identifying a resource.
                var values = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                await using NpgsqlCommand resourceCommand = _conn.CreateCommand();
                for (int i = 0; i < consentRight.Resource.Count; i++)
                {
                    values.Add($"(@consentRightId{i}, @type{i}, @value{i})");
                    resourceCommand.Parameters.AddWithValue($"@consentRightId{i}", consentRightGuid);
                    resourceCommand.Parameters.AddWithValue($"@type{i}", consentRight.Resource[i].Type);
                    resourceCommand.Parameters.AddWithValue($"@value{i}", consentRight.Resource[i].Value);
                }

                resourceCommand.CommandText = $"INSERT INTO consent.resourceattribute (consentRightId, type, value) VALUES {string.Join(", ", values)}";
                await resourceCommand.ExecuteNonQueryAsync();
            }

            return await GetRequest(consentRequest.Id);
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
                WHERE consentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentQuery);
            pgcom.Parameters.AddWithValue("id", consentRequestId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                Guid from = await reader.GetFieldValueAsync<Guid>("fromPartyUuid");
                Guid to = await reader.GetFieldValueAsync<Guid>("toPartyUuid");

                ConsentPartyUrn fromPartyUrn = ConsentPartyUrn.PartyUuid.Create(from);
                ConsentPartyUrn toPartyUrn = ConsentPartyUrn.PartyUuid.Create(to);

                if (fromPartyUrn == null || toPartyUrn == null)
                {
                    throw new InvalidDataException("Invalid party URN");
                }

                return new ConsentRequestDetails
                {
                    Id = consentRequestId,
                    From = fromPartyUrn,
                    To = toPartyUrn,
                    ValidTo = await reader.GetFieldValueAsync<DateTimeOffset>("validTo"),
                    ConsentRights = consentRight,
                    Requestmessage = await reader.GetFieldValueAsync<Dictionary<string, string>>("requestMessage"),
                    ConsentRequestStatus = await reader.GetFieldValueAsync<ConsentRequestStatusType>("status"),
                    Consented = await reader.GetFieldValueAsync<DateTimeOffset?>("consented")
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
                WHERE consentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentRightsQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);
            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            List<ConsentRight> consentRights = new List<ConsentRight>();
            while (await reader.ReadAsync())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("consentRightId");

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
                join consent.consentright cr on cr.consentRightId = ra.consentRightId 
                WHERE cr.consentRequestId = @id
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
                WHERE cr.consentRequestId = @id
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
