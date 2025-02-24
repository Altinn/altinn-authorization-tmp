using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Models.Register;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Persistence.Configuration;
using Altinn.Register.Core.Parties;
using Microsoft.Extensions.Options;
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
        public async Task<ConsentRequestDetails> GetRequest(Guid id)
        {
            string consentQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentrequest 
                WHERE id = @id
                ";

            string consentRightsQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentright 
                WHERE concentRequestId = @id
                ";

            string consentResourcesQuery = /*strpsql*/@$"
                SELECT * FROM consent.resourceattributes ra 
                join consent.concentright cr on cr.consentRightId = ra.consentRightId 
                WHERE cr.concentRequestId = @id
                ";

            string consentMetadataQuery = /*strpsql*/@$"
                SELECT * FROM consent.metadata ra 
                join consent.consentright cr on cr.consentRightId = ra.consentRightId
                WHERE cr.concentRequestId = @id
                ";

            await using var pgcom = _conn.CreateCommand(consentRightsQuery);
            pgcom.Parameters.AddWithValue("_altinnAppId", altinnAppId);

            await using var cmd = _conn.CreateCommand(query);
            cmd.Parameters.AddWithValue("altinnAppId", NpgsqlDbType.Text, altinnAppId);


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



        private async Task<List<ConsentRight>> GetConsentRights(Guid consentId)
        {
            string consentRightsQuery = /*strpsql*/@$"
                SELECT * FROM consent.consentright 
                WHERE concentRequestId = @id
                ";


            await using var pgcom = _conn.CreateCommand(consentRightsQuery);
            pgcom.Parameters.AddWithValue("_id", consentId);
            using NpgsqlDataReader reader = await pgcom.ExecuteReaderAsync();
            List<ConsentRight> consentRights = new List<ConsentRight>();
            while (await reader.ReadAsync())
            {
                consentRights.Add(new ConsentRight
                {
                    ConsentRightId = reader.GetFieldValue<Guid>("consentRightId"),
                    ConsentRequestId = reader.GetFieldValue<Guid>("concentRequestId"),
                    ConsentType = Enum.Parse<ConsentType>(reader.GetFieldValue<string>("consentType")),
                    Created = reader.GetFieldValue<DateTime>("created"),
                    Modified = reader.GetFieldValue<DateTime>("modified")
                });
            }
            return consentRights;
        }
}
