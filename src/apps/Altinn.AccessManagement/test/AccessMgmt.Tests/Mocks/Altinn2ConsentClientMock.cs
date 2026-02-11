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
using Altinn.AccessManagement.Core.Constants;
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

            GetA2Request(Guid.Parse(consentList.First()));

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
                EventType = ConsentRequestEventType.Used,
                PerformedBy = result.From
            };

            consentEvents.Add(consentEvent2);

            ConsentRequestEvent consentEvent3 = new()
            {
                ConsentRequestID = id,
                Created = DateTimeOffset.UtcNow,
                EventType = ConsentRequestEventType.Accepted,
                PerformedBy = result.From
            };

            consentEvents.Add(consentEvent3);

            result.ConsentRequestEvents = consentEvents;

            return result;
        }

        private ConsentRequest GetA2Request(Guid id)
        {
            Stream dataStream = File.OpenRead($"Data/Consent/a2consent_request_{id.ToString()}.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            Altinn2ConsentRequest result = JsonSerializer.Deserialize<Altinn2ConsentRequest>(dataStream, options);

            ConsentRequest consentRequest = MapA2ConsentToA3Consent(result, CancellationToken.None).GetAwaiter().GetResult();

            return consentRequest;
        }

        private async Task<ConsentRequest> MapA2ConsentToA3Consent(Altinn2ConsentRequest altinn2Consent, CancellationToken cancellationToken)
        {
            ConsentRequest consent = new ConsentRequest
            {
                Id = altinn2Consent.ConsentGuid,
                CreatedTime = altinn2Consent.CreatedTime,
                From = ConsentPartyUrn.PartyUuid.Create((Guid)altinn2Consent.OfferedByPartyUUID),
                To = ConsentPartyUrn.PartyUuid.Create((Guid)altinn2Consent.CoveredByPartyUUID),
                ValidTo = altinn2Consent.ValidTo,
                ConsentRights = await MapAltinn2ResourcesToConsentRights(altinn2Consent.RequestResources, cancellationToken),
                ConsentRequestEvents = await MapA2ConsentEventsToA3ConsentEvents(altinn2Consent.ConsentHistoryEvents, cancellationToken),
                RedirectUrl = altinn2Consent.RedirectUrl,
                TemplateId = altinn2Consent.TemplateId
            };

            return consent;
        }

        private async Task<List<ConsentRight>> MapAltinn2ResourcesToConsentRights(List<AuthorizationRequestResourceBE> resources, CancellationToken cancellationToken)
        {
            List<ConsentRight> consentRights = new();

            foreach (AuthorizationRequestResourceBE resource in resources)
            {
                ConsentRight consentRight = new()
                {
                    Action = resource.Operations,
                    Resource = new List<ConsentResourceAttribute>(),
                    Metadata = new MetadataDictionary()
                };

                consentRight.AddMetadataValues(resource.Metadata);

                string searchParam = $"reference={resource.ServiceEditionVersionID}&ResourceType=Consent&id={resource.ServiceCode}_{resource.ServiceEditionCode}";
                ConsentResourceAttribute consentResourceAttribute = new()
                {
                    Type = AltinnXacmlConstants.MatchAttributeIdentifiers.ResourceRegistryAttribute,
                    Value = "Identifier",
                    Version = "123"
                };

                consentRight.Resource.Add(consentResourceAttribute);
            }

            return await Task.FromResult(consentRights);
        }

        private async Task<List<ConsentRequestEvent>> MapA2ConsentEventsToA3ConsentEvents(List<Altinn2ConsentRequestEvent> a2Events, CancellationToken cancellationToken)
        {
            List<ConsentRequestEvent> consentEvents = new();

            foreach (Altinn2ConsentRequestEvent a2Event in a2Events)
            {
                ConsentRequestEvent consentEvent = new()
                {
                    ConsentRequestID = a2Event.ConsentRequestID,
                    Created = a2Event.Created,
                    EventType = Enum.Parse<ConsentRequestEventType>(a2Event.EventType),
                    PerformedBy = ConsentPartyUrn.PartyUuid.Create(a2Event.PerformedByPartyUUID ?? Guid.Empty)
                };

                consentEvents.Add(consentEvent);
            }

            return await Task.FromResult(consentEvents);
        }
    }
}
