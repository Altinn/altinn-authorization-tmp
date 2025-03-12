using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.Consent;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using System.Text.Json;

namespace AccessMgmt.Tests.Mocks
{
    public class ConsentRepositoryMock : IConsentRepository
    {
        public Task ApproveConsentRequest(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ConsentRequestDetails> CreateRequest(ConsentRequest consentRequest, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteRequest(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<List<Consent>> GetAllConsents(Guid partyUid, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<Consent> GetConsent(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<ConsentRequestDetails> GetRequest(Guid id, CancellationToken cancellationToken = default)
        {
            Stream dataStream = File.OpenRead($"Data/Consent/consent_request_{id.ToString()}.json");
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ConsentRequestDetails result = JsonSerializer.Deserialize<ConsentRequestDetails>(dataStream, options);
            return Task.FromResult(result);
        }

        public Task RejectConsentRequest(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Revoke(Guid id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
