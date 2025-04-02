using System.Data;
using System.Security.Policy;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.Authorization.Core.Models.Consent;
using Azure.Core;
using Microsoft.AspNetCore.Http.HttpResults;
using Npgsql;
using NpgsqlTypes;

namespace Altinn.AccessManagement.Persistence.Consent
{
    /// <summary>
    /// Repository for handling consent data.
    /// </summary>
    public class ConsentRepository : IConsentRepository
    {
        private readonly NpgsqlDataSource _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsentRepository"/> class
        /// </summary>
        public ConsentRepository(NpgsqlDataSource db)
        {
            _db = db;
        }

        /// <inheritdoc/>
        public async Task AcceptConsentRequest(Guid consentRequestId, Guid performedByParty,  CancellationToken cancellationToken = default)
        {
            DateTimeOffset consentedTime = DateTime.UtcNow;

            const string updateConsentRequestQuery = /*strpsql*/@"
                    UPDATE consent.consentrequest set status = 'accepted', consented = @consentedTime  WHERE consentRequestId= @consentRequestId and status = 'created'";

            await using NpgsqlConnection conn = await _db.OpenConnectionAsync(default);

            // Run all inserts in one transaction in case of failure
            await using NpgsqlTransaction tx = await conn.BeginTransactionAsync();
            await using NpgsqlCommand command = conn.CreateCommand();
            command.CommandText = updateConsentRequestQuery;
            command.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            command.Parameters.AddWithValue("consentedTime", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                // No rows were updated, meaning the consent request ID was not found or the status was not created 
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            const string eventQuery = /*strpsql*/@"
                INSERT INTO consent.consentevent (consentEventId, consentRequestId, eventtype, created, performedByParty)
                VALUES (
                @consentEventId, 
                @consentRequestId, 
                @eventtype, 
                @created, 
                @performedByParty)
                RETURNING consentEventId;
                ";

            await using NpgsqlCommand eventCommand = conn.CreateCommand();
            eventCommand.CommandText = eventQuery;
            eventCommand.Parameters.AddWithValue("consentEventId", NpgsqlDbType.Uuid, Guid.CreateVersion7());
            eventCommand.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            eventCommand.Parameters.Add(new NpgsqlParameter<ConsentRequestEventType>("eventtype", ConsentRequestEventType.Created));
            eventCommand.Parameters.AddWithValue("created", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            eventCommand.Parameters.AddWithValue("performedByParty", NpgsqlDbType.Uuid, performedByParty);
            await eventCommand.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest, Guid performedByParty, CancellationToken cancellationToken = default)
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

            await using NpgsqlConnection conn = await _db.OpenConnectionAsync(default);

            // Run all inserts in one transaction in case of failure
            await using NpgsqlTransaction tx = await conn.BeginTransactionAsync();
            await using NpgsqlCommand command = conn.CreateCommand();
            command.CommandText = consentRquestQuery;
            command.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid,  consentRequest.Id);
            if (consentRequest.From.IsPartyUuid(out Guid fromPartyGuid))
            {
                command.Parameters.AddWithValue("fromPartyUuid", NpgsqlDbType.Uuid, fromPartyGuid);
            }
            else
            {
                throw new InvalidDataException("Invalid fromPartyUuid");
            }

            if (consentRequest.To.IsPartyUuid(out Guid toPartyGuid))
            {
                command.Parameters.AddWithValue("toPartyUuid", NpgsqlDbType.Uuid, toPartyGuid);
            }
            else
            {
                throw new InvalidDataException("Invalid toPartyUuid");
            }

            command.Parameters.AddWithValue("requestMessage", NpgsqlDbType.Hstore, consentRequest.Requestmessage);

            command.Parameters.AddWithValue("validTo", NpgsqlDbType.TimestampTz, consentRequest.ValidTo.ToOffset(TimeSpan.Zero));
            await command.ExecuteNonQueryAsync();

            foreach (ConsentRight consentRight in consentRequest.ConsentRights)
            {
                Guid consentRightGuid = Guid.CreateVersion7();

                const string rightsQuery = /*strpsql*/@"
                INSERT INTO consent.consentright(consentRightId , consentRequestId , action)
                VALUES (
                @consentRightId, 
                @consentRequestId, 
                @action
                )
                RETURNING consentRightId;
                ";

                await using NpgsqlCommand rightsCommand = conn.CreateCommand();
                rightsCommand.CommandText = rightsQuery;
                rightsCommand.Parameters.AddWithValue("consentRightId", consentRightGuid);
                rightsCommand.Parameters.AddWithValue("consentRequestId", consentRequest.Id);
                rightsCommand.Parameters.AddWithValue("action", consentRight.Action);
                await rightsCommand.ExecuteNonQueryAsync();

                // Bulding up the query for the resource attributes. Typical this is only one, but in theory it can be multiple attributes identifying a resource.
                var values = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                await using NpgsqlCommand resourceCommand = conn.CreateCommand();
                for (int i = 0; i < consentRight.Resource.Count; i++)
                {
                    values.Add($"(@consentRightId{i}, @type{i}, @value{i})");
                    resourceCommand.Parameters.AddWithValue($"@consentRightId{i}", consentRightGuid);
                    resourceCommand.Parameters.AddWithValue($"@type{i}", consentRight.Resource[i].Type);
                    resourceCommand.Parameters.AddWithValue($"@value{i}", consentRight.Resource[i].Value);
                }

                resourceCommand.CommandText = $"INSERT INTO consent.resourceattribute (consentRightId, type, value) VALUES {string.Join(", ", values)}";
                await resourceCommand.ExecuteNonQueryAsync(cancellationToken);

                if (consentRight.MetaData != null && consentRight.MetaData.Count > 0)
                {
                    await using NpgsqlCommand metadatacommand = conn.CreateCommand();
                    List<string> metaValues = [];
                    int metaDataIndex = 0;
                    foreach (KeyValuePair<string, string> kvp in consentRight.MetaData)
                    {
                        metaValues.Add($"(@consentRightId{metaDataIndex}, @id{metaDataIndex}, @value{metaDataIndex})");
                        metadatacommand.Parameters.AddWithValue($"@consentrightid{metaDataIndex}", consentRightGuid);
                        metadatacommand.Parameters.AddWithValue($"@id{metaDataIndex}", kvp.Key);
                        metadatacommand.Parameters.AddWithValue($"@value{metaDataIndex}", kvp.Value);
                        metaDataIndex++;
                    }

                    metadatacommand.CommandText = $"INSERT INTO consent.metadata (consentrightid, id, value) VALUES {string.Join(", ", metaValues)}";
                    await metadatacommand.ExecuteNonQueryAsync(cancellationToken);
                }
            }

            await tx.CommitAsync(); 

            return await GetRequest(consentRequest.Id);
        }

        /// <inheritdoc/>
        public Task DeleteRequest(Guid id, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<List<Altinn.Authorization.Core.Models.Consent.Consent>> GetAllConsents(Guid partyUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ConsentRequestDetails> GetRequest(Guid consentRequestId, CancellationToken cancellationToken = default)
        {
            List<ConsentRight> consentRight = await GetConsentRights(consentRequestId);

            string consentQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentrequest 
                WHERE consentRequestId = @id
                ";

            await using var pgcom = _db.CreateCommand(consentQuery);
            pgcom.Parameters.AddWithValue("id", consentRequestId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);

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
        public async Task RejectConsentRequest(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            DateTimeOffset consentedTime = DateTime.UtcNow;

            const string updateConsentRequestQuery = /*strpsql*/@"
                    UPDATE consent.consentrequest set status = 'rejected' WHERE consentRequestId= @consentRequestId and status = 'created'";

            await using NpgsqlConnection conn = await _db.OpenConnectionAsync(default);

            // Run all inserts in one transaction in case of failure
            await using NpgsqlTransaction tx = await conn.BeginTransactionAsync();
            await using NpgsqlCommand command = conn.CreateCommand();
            command.CommandText = updateConsentRequestQuery;
            command.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            command.Parameters.AddWithValue("consentedTime", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                // No rows were updated, meaning the consent request ID was not found or the status was not created 
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            const string eventQuery = /*strpsql*/@"
                INSERT INTO consent.consentevent (consentEventId, consentRequestId, eventtype, created, performedByParty)
                VALUES (
                @consentEventId, 
                @consentRequestId, 
                @eventtype, 
                @created, 
                @performedByParty)
                RETURNING consentEventId;
                ";

            await using NpgsqlCommand eventCommand = conn.CreateCommand();
            eventCommand.CommandText = eventQuery;
            eventCommand.Parameters.AddWithValue("consentEventId", NpgsqlDbType.Uuid, Guid.CreateVersion7());
            eventCommand.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            eventCommand.Parameters.Add(new NpgsqlParameter<ConsentRequestEventType>("eventtype", ConsentRequestEventType.Rejected));
            eventCommand.Parameters.AddWithValue("created", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            eventCommand.Parameters.AddWithValue("performedByParty", NpgsqlDbType.Uuid, performedByParty);
            await eventCommand.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }

        /// <inheritdoc/>
        public async Task Revoke(Guid consentRequestId, Guid performedByParty, CancellationToken cancellationToken = default)
        {
            DateTimeOffset consentedTime = DateTime.UtcNow;

            const string updateConsentRequestQuery = /*strpsql*/@"
                    UPDATE consent.consentrequest set status = 'revoked', consented = @consentedTime  WHERE consentRequestId= @consentRequestId and status = 'accepted'";

            await using NpgsqlConnection conn = await _db.OpenConnectionAsync(default);

            // Run all inserts in one transaction in case of failure
            await using NpgsqlTransaction tx = await conn.BeginTransactionAsync();
            await using NpgsqlCommand command = conn.CreateCommand();
            command.CommandText = updateConsentRequestQuery;
            command.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            command.Parameters.AddWithValue("consentedTime", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected == 0)
            {
                // No rows were updated, meaning the consent request ID was not found or the status was not created 
                throw new InvalidOperationException($"Consent request with ID {consentRequestId} not found or already updated.");
            }

            const string eventQuery = /*strpsql*/@"
                INSERT INTO consent.consentevent (consentEventId, consentRequestId, eventtype, created, performedByParty)
                VALUES (
                @consentEventId, 
                @consentRequestId, 
                @eventtype, 
                @created, 
                @performedByParty)
                RETURNING consentEventId;
                ";

            await using NpgsqlCommand eventCommand = conn.CreateCommand();
            eventCommand.CommandText = eventQuery;
            eventCommand.Parameters.AddWithValue("consentEventId", NpgsqlDbType.Uuid, Guid.CreateVersion7());
            eventCommand.Parameters.AddWithValue("consentRequestId", NpgsqlDbType.Uuid, consentRequestId);
            eventCommand.Parameters.Add(new NpgsqlParameter<ConsentRequestEventType>("eventtype", ConsentRequestEventType.Revoked));
            eventCommand.Parameters.AddWithValue("created", NpgsqlDbType.TimestampTz, consentedTime.ToOffset(TimeSpan.Zero));
            eventCommand.Parameters.AddWithValue("performedByParty", NpgsqlDbType.Uuid, performedByParty);
            await eventCommand.ExecuteNonQueryAsync();
            await tx.CommitAsync();
        }

        /// <summary>
        /// Return the consent rights for a given consentRequest
        /// </summary>
        private async Task<List<ConsentRight>> GetConsentRights(Guid consentRequestId, CancellationToken cancellationToken = default)
        {
            Dictionary<Guid, List<ConsentResourceAttribute>> keyValuePairs = await GetConsentResourceAttributes(consentRequestId, cancellationToken);

            string consentRightsQuery = /*strpsql*/@$"
                SELECT 
                consentRightId,
                consentRequestId,
                action 
                FROM consent.consentright 
                WHERE consentRequestId = @consentRequestId
                ";

            await using var pgcom = _db.CreateCommand(consentRightsQuery);
            pgcom.Parameters.AddWithValue("@consentRequestId", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);
            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);
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
                Dictionary<Guid, Dictionary<string, string>> consentMetadata = await GetConsentRightMetadata(consentRequestId, cancellationToken);
                if (consentMetadata.TryGetValue(consentRightId, out Dictionary<string, string> foundMetadata))
                {
                    metadata = foundMetadata;
                }
                else
                {
                    metadata = null;
                }

                ConsentRight consentRight = new ConsentRight
                {
                    Action = reader.GetFieldValue<List<string>>("action"),
                    Resource = resourceAttributes
                };

                consentRight.SetMetadataValues(metadata);

                consentRights.Add(consentRight);
            }

            return consentRights;
        }

        /// <summary>
        ///  Gets the consent resource attributes for a consent request. Returned as a dictinary to be able to group the attributes by consent right.
        /// </summary>
        private async Task<Dictionary<Guid, List<ConsentResourceAttribute>>> GetConsentResourceAttributes(Guid consentRequestId, CancellationToken cancellationToken)
        {
            string consentResourcesQuery = /*strpsql*/@$"
                SELECT 
                cr.consentRightId,
                type,
                value 
                FROM consent.resourceattribute ra 
                join consent.consentright cr on cr.consentRightId = ra.consentRightId 
                WHERE cr.consentRequestId = @id
                ";

            await using var pgcom = _db.CreateCommand(consentResourcesQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);
            Dictionary<Guid, List<ConsentResourceAttribute>> keyValuePairs = new Dictionary<Guid, List<ConsentResourceAttribute>>();

            while (reader.Read())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("consentRightId");
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

        private async Task<Dictionary<Guid, Dictionary<string, string>>> GetConsentRightMetadata(Guid consentRequestId, CancellationToken cancellationToken)
        {
            string consentMetadataQuery = /*strpsql*/@$"
                SELECT
                cr.consentRightId,
                id,
                value 
                FROM consent.metadata ra 
                join consent.consentright cr on cr.consentRightId = ra.consentRightId
                WHERE cr.consentRequestId = @id
                ";

            await using var pgcom = _db.CreateCommand(consentMetadataQuery);
            pgcom.Parameters.AddWithValue("@id", NpgsqlTypes.NpgsqlDbType.Uuid, consentRequestId);
            Dictionary<Guid, Dictionary<string, string>> consentMetadata = [];

            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync(cancellationToken);
            while (reader.Read())
            {
                Guid consentRightId = reader.GetFieldValue<Guid>("consentRightId");
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
