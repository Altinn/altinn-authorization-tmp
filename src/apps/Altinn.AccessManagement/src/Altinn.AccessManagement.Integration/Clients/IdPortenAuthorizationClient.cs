using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Models.IdPortenAuthorization;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Client for getting IdPorten authorizations from IdPorten API
/// </summary>
[ExcludeFromCodeCoverage]
public class IdPortenAuthorizationClient : IIdPortenAuthorizationClient
{
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly PlatformSettings _platformSettings;
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="PartiesClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default httpclientfactory</param>
    /// <param name="logger">the logger</param>
    /// <param name="platformSettings">the platform setttings</param>
    /// <param name="accessTokenGenerator">An instance of the AccessTokenGenerator service.</param>
    public IdPortenAuthorizationClient(
        HttpClient httpClient,
        ILogger<IdPortenAuthorizationClient> logger,
        IOptions<PlatformSettings> platformSettings,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _logger = logger;
        _client = httpClient;
        _platformSettings = platformSettings.Value;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc/>
    public async Task<IdPortenClientResult<List<IdPortenAuthorization>>> GetIdPortenAuthorizations(string ssn, CancellationToken cancellationToken)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_platformSettings.IdPortenApiEndpoint}api-provider/authorizations");

            var body = new { ssn = ssn };
            string json = JsonSerializer.Serialize(body, _serializerOptions);
            StringContent requestBody = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(uriBuilder.Uri, requestBody, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var value = JsonSerializer.Deserialize<List<IdPortenAuthorization>>(responseContent, _serializerOptions);
                return new(response.StatusCode, value);
            }

            _logger.LogError("AccessManagement // IdPortenAuthorizationClient // GetIdPortenAuthorizations // Unexpected HttpStatusCode: {StatusCode}\n {ResponseContent}", response.StatusCode, responseContent);
            return new(response.StatusCode, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // IdPortenAuthorizationClient // GetIdPortenAuthorizations // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<IdPortenClientResult<bool>> DeleteIdPortenAuthorization(string ssn, string id, CancellationToken cancellationToken)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_platformSettings.IdPortenApiEndpoint}api-provider/authorizations/{id}");

            var body = new { ssn = ssn };
            string json = JsonSerializer.Serialize(body, _serializerOptions);
            StringContent requestBody = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Delete, uriBuilder.Uri)
            {
                Content = requestBody
            };

            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new(response.StatusCode, true);
            }

            _logger.LogError("AccessManagement // IdPortenAuthorizationClient // DeleteIdPortenAuthorization // Unexpected HttpStatusCode: {StatusCode}\n {ResponseContent}", response.StatusCode, responseContent);
            return new(response.StatusCode, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // IdPortenAuthorizationClient // DeleteIdPortenAuthorization // Exception");
            throw;
        }
    }
}
