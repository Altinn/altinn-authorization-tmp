using System.Text.Json;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.IdPortenAuthorization;

namespace Altinn.AccessManagement.Tests.Mocks
{
    public class IdPortenAuthorizationClientMock : IIdPortenAuthorizationClient
    {
        public Task<List<IdPortenAuthorization>> GetIdPortenAuthorizations(string ssn, CancellationToken cancellationToken)
        {
            string unitTestFolder = Path.GetDirectoryName(new Uri(typeof(AMPartyServiceMock).Assembly.Location).LocalPath);
            string partiesPath = Path.Combine(unitTestFolder, "Data", "IdPortenAuthorization", "IdPortenAuthorization_list.json");
            string content = File.ReadAllText(partiesPath);
            List<IdPortenAuthorization> results = JsonSerializer.Deserialize<List<IdPortenAuthorization>>(content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return Task.FromResult(results);
        }

        public Task<bool> DeleteIdPortenAuthorization(string ssn, string id, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
