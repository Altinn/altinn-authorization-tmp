using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Models.Profile;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Profile.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients
{
    /// <summary>
    /// A client for retrieving profiles from Altinn Platform.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ProfileClient : IProfileClient
    {
        private readonly ILogger _logger;
        private readonly PlatformSettings _settings;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        private readonly IAccessTokenGenerator _accessTokenGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileClient"/> class
        /// </summary>
        /// <param name="platformSettings">the platform settings</param>
        /// <param name="logger">the logger</param>
        /// <param name="httpClient">A HttpClient provided by the HttpClientFactory.</param>
        /// <param name="accessTokenGenerator">PlatformAccessToken generator</param>
        public ProfileClient(
            IOptions<PlatformSettings> platformSettings,
            ILogger<ProfileClient> logger,
            HttpClient httpClient,
            IAccessTokenGenerator accessTokenGenerator)
        {
            _logger = logger;
            _settings = platformSettings.Value;
            _client = httpClient;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
            _accessTokenGenerator = accessTokenGenerator;
        }

        /// <inheritdoc/>
        public async Task<UserProfile> GetUser(UserProfileLookup userProfileLookup, CancellationToken cancellationToken = default)
        {
            UriBuilder endpoint = new UriBuilder($"{_settings.ApiProfileEndpoint}internal/user/");

            StringContent requestBody = new StringContent(JsonSerializer.Serialize(userProfileLookup), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(endpoint.Uri, requestBody, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Getting user profile failed with unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
                return null;
            }
            
            UserProfile userProfile = JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptions);

            return userProfile;
        }

        /// <inheritdoc/>
        public async Task<NewUserProfile> GetNewUserProfile(int userId, CancellationToken cancellationToken = default)
        {
            var endpointUrl = new UriBuilder($"{_settings.ApiProfileEndpoint}users/{userId}");
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl.Uri.ToString(), null, accessToken, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NewUserProfile>(cancellationToken);
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AccessManagement // ProfileClient // GetNewUserProfile  // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
                return null;
            }
        }
    }
}
