using System.Net.Http.Json;
using System.Web;
using Altinn.Authorization.Integration.Register.Models;

namespace Altinn.Authorization.Integration.Register;

/// <inheritdoc/>
public partial class RegisterClient : IAltinnRegister
{
    private readonly IEnumerable<string> _availableFields = [
        "party",
        "organization",
        "person",
        "identifiers",
        "party-uuid",
        "party-version-id",
        "party-is-deleted",
        "organization-business-address",
        "organization-mailing-address",
        "organization-internet-address",
        "organization-email-address",
        "organization-fax-number",
        "organization-mobile-number",
        "organization-telephone-number",
        "organization-unit-type",
        "organization-unit-status",
        "person-date-of-death",
        "person-mailing-address",
        "person-address",
        "person-last-name",
        "person-middle-name",
        "person-first-name",
        "party-modified-at",
        "party-created-at",
        "party-organization-identifier",
        "party-person-identifier",
        "party-name",
        "party-type",
        "party-id",
        "person-date-of-birth",
        "sub-units"
    ];

    /// <inheritdoc/>
    public async Task<IAsyncEnumerable<Paginated<PartyModel>>> Stream(string nextPage, IEnumerable<string> fields, CancellationToken cancellationToken)
    {
        var request = NewStreamRequest(nextPage, fields);
        var response = await HttpClient.SendAsync(request, cancellationToken);
        return new PartyStream(response, HttpClient, NewStreamRequest);
    }

    private HttpRequestMessage NewStreamRequest(string nextPage, IEnumerable<string> fields)
    {
        if (fields.Distinct().All(_availableFields.Contains))
        {
            throw new ArgumentException("Some or all provided fields is not retrievable from register", nameof(fields));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, nextPage);
        if (string.IsNullOrEmpty(nextPage))
        {
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["fields"] = string.Join(",", fields);
            request.RequestUri = new UriBuilder(Options.Value.Endpoint)
            {
                Query = query.ToString()
            }.Uri;
        }

        AddAuthorization(request);

        return request;
    }

    /// <summary>
    /// Stream Parties
    /// </summary>
    /// <param name="response">Initial HTTP response</param>
    /// <param name="httpClient">Http client</param>
    /// <param name="newRequest"></param>
    public class PartyStream(HttpResponseMessage response, HttpClient httpClient, Func<string, IEnumerable<string>, HttpRequestMessage> newRequest) : IAsyncEnumerable<Paginated<PartyModel>>
    {
        private HttpResponseMessage Response { get; } = response;

        private HttpClient HttpClient { get; } = httpClient;

        private Func<string, IEnumerable<string>, HttpRequestMessage> NewRequest { get; } = newRequest;

        /// <inheritdoc/>
        public async IAsyncEnumerator<Paginated<PartyModel>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var response = Response;
            while (!cancellationToken.IsCancellationRequested)
            {
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<Paginated<PartyModel>>(cancellationToken);
                    var dispatchNextPage = FetchNextPage(content, cancellationToken);

                    if (content.Items.Any())
                    {
                        yield return content;
                    }

                    response.Dispose();
                    response = await dispatchNextPage;
                    if (response == null)
                    {
                        yield break;
                    }
                }
            }
        }

        private async Task<HttpResponseMessage> FetchNextPage(Paginated<PartyModel> content, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(content.Links.Next))
            {
                return null;
            }

            var request = NewRequest(content.Links.Next, []);
            return await HttpClient.SendAsync(request, cancellationToken);

        }
    }
}

/// <summary>
/// Altinn Register
/// </summary>
public partial interface IAltinnRegister
{
    /// <summary>
    /// Creates a stream that can be iterated over
    /// </summary>
    /// <param name="nextPage">next page url, set to string.empty null to start from the beginning</param>
    /// <param name="fields">which fields to get from the request</param>
    /// <param name="cancellationToken">cancellation token</param>
    Task<IAsyncEnumerable<Paginated<PartyModel>>> Stream(string nextPage, IEnumerable<string> fields, CancellationToken cancellationToken);
}
