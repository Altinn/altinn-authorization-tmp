using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Enums;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Integration.Clients;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace Altinn.AccessManagement.Tests.Mocks
{
    /// <summary>
    /// Mock class for <see cref="IAltinn2ConsentClient"></see> interface
    /// </summary>
    public class Altinn2ConsentClientMock : IAltinn2ConsentClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Altinn2ConsentClientMock"/> class
        /// </summary>
        public Altinn2ConsentClientMock()
        {
        }

        /// <inheritdoc/>
        public Task<ConsentRequest> GetConsent(Guid consentGuid, CancellationToken cancellationToken = default)
        {
            ConsentRequest request = GetRequest(consentGuid);

            return Task.FromResult(request);
        }

        /// <inheritdoc/>
        public Task<List<Guid>> GetConsentListForMigration(int numberOfConsentsToReturn, int? status, bool onlyGetExpired, CancellationToken cancellationToken = default)
        {
            List<Guid> list = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            return Task.FromResult(list);
        }

        /// <inheritdoc/>
        public Task<List<ConsentRequest>> GetMultipleConsents(List<string> consentList, CancellationToken cancellationToken = default)
        {
            List<ConsentRequest> consents = consentList.Select(id => new ConsentRequest
            {
                Id = Guid.Parse(id),
                From = ConsentPartyUrn.PartyUuid.Create(Guid.NewGuid()),
                To = ConsentPartyUrn.PartyUuid.Create(Guid.NewGuid()),
                ValidTo = DateTimeOffset.UtcNow.AddDays(30),
                RequestMessage = new Dictionary<string, string> { { "Test", "TestValue" } },
                Consented = DateTimeOffset.UtcNow,
                TemplateId = "TestTemplate",
                CreatedTime = DateTimeOffset.UtcNow.AddDays(-1), 
                RedirectUrl = "https://example.com/redirect",
                ConsentRights = new List<ConsentRight>()
            }).ToList();

            return Task.FromResult(consents);
        }

        public Task<bool> UpdateConsentMigrateStatus(string consentGuid, int status, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        private ConsentRequest GetRequest(Guid id)
        {
            Stream dataStream = File.OpenRead($"Data/Consent/consent_request_{id.ToString()}.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ConsentRequest result = JsonSerializer.Deserialize<ConsentRequest>(dataStream, options);

            List<ConsentRequestEvent> consentEvents = new();

            ConsentRequestEvent consentEvent = new()
            {
                ConsentRequestID = id,
                Created = DateTimeOffset.UtcNow,
                EventType = ConsentRequestEventType.Created,
                PerformedBy = result.From
            };

            consentEvents.Add(consentEvent);

            ConsentRequestEvent consentEvent2 = new()
            {
                ConsentRequestID = id,
                Created = DateTimeOffset.UtcNow,
                EventType = ConsentRequestEventType.Accepted,
                PerformedBy = result.From
            };

            consentEvents.Add(consentEvent2);

            return result;
        }
    }
}
