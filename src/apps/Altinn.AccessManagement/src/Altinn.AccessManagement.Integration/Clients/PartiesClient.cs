using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.AccessManagement.Core.Clients.Interfaces;
using Altinn.AccessManagement.Core.Extensions;
using Altinn.AccessManagement.Core.Helpers;
using Altinn.AccessManagement.Core.Models.SblBridge;
using Altinn.AccessManagement.Integration.Configuration;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Platform.Register.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.AccessManagement.Integration.Clients;

/// <summary>
/// Proxy implementation for parties
/// </summary>
[ExcludeFromCodeCoverage]
public class PartiesClient : IPartiesClient
{
    private readonly SblBridgeSettings _sblBridgeSettings;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PlatformSettings _platformSettings;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Initializes a new instance of the <see cref="PartiesClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default httpclientfactory</param>
    /// <param name="sblBridgeSettings">the sbl bridge settings</param>
    /// <param name="logger">the logger</param>
    /// <param name="httpContextAccessor">handler for http context</param>
    /// <param name="platformSettings">the platform setttings</param>
    /// <param name="accessTokenGenerator">An instance of the AccessTokenGenerator service.</param>
    public PartiesClient(
        HttpClient httpClient, 
        IOptions<SblBridgeSettings> sblBridgeSettings, 
        ILogger<PartiesClient> logger, 
        IHttpContextAccessor httpContextAccessor, 
        IOptions<PlatformSettings> platformSettings,
        IAccessTokenGenerator accessTokenGenerator)
    {
        _sblBridgeSettings = sblBridgeSettings.Value;
        _logger = logger;
        httpClient.BaseAddress = new Uri(platformSettings.Value.ApiRegisterEndpoint);
        httpClient.DefaultRequestHeaders.Add(platformSettings.Value.SubscriptionKeyHeaderName, platformSettings.Value.SubscriptionKey);
        _client = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _platformSettings = platformSettings.Value;
        _accessTokenGenerator = accessTokenGenerator;
        _serializerOptions.Converters.Add(new JsonStringEnumConverter());
    }

    /// <inheritdoc/>
    public async Task<Party> GetPartyAsync(int partyId, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpointUrl = $"parties/{partyId}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl, token, accessToken, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<Party>(responseContent, _serializerOptions);
            }
            
            _logger.LogError("AccessManagement // PartiesClient // GetPartyAsync // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartyAsync // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Party>> GetPartiesAsync(List<int> partyIds, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpointUrl = $"parties/partylist?fetchSubUnits={includeSubunits}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
            accessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkJCQjA2MkM5ODI4NEZBRTYxOUNCMjlGRkYyQ0FBMDFGNUE3QzU2RjIiLCJ0eXAiOiJKV1QiLCJ4NWMiOiJCQkIwNjJDOTgyODRGQUU2MTlDQjI5RkZGMkNBQTAxRjVBN0M1NkYyIn0.eyJ1cm46YWx0aW5uOmFwcCI6InNibC5hdXRob3JpemF0aW9uIiwiZXhwIjoyMTI5NDI1MDQyLCJpYXQiOjE3Njk0MjUwNDIsImlzcyI6InBsYXRmb3JtIiwiYWN0dWFsX2lzcyI6ImFsdGlubi10ZXN0LXRvb2xzIiwibmJmIjoxNzY5NDI1MDQyfQ.E94FvkxfLo0fKUH8j3tEOtSHrcM4tan_Czdn3llSWVt4mMK0YJytljrLnXRugVKqa2ZP1x1Rs0XUhc2stsC73BZHQ7UkQ05OGb0FEqBxC6nq8rM74AdV8HTl0-PasXDpPkrzX-naPUvbtOIZgjMBA-E7tGvBoVpicuNkY1JPPtzAqWbRL6SXFHzT3P8n534ZG--r8XUWe-HBloHh765-__YTC74M0M-d1ZmY5ZkzfXBCNIqIo4OTa_HPBGiUSyGEiiA1dlgYb2JqEyn0Y1ASRA8F0M7mn3gZ7PGMtlYsouD9SheH4iQp1X3ObtbOsT2hW6dQhBaujWJcpA7vzouJLg";
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(partyIds), Encoding.UTF8, "application/json");
            
            HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<List<Party>>(responseContent, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // GetPartiesAsync // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartiesAsync // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Party>> GetPartiesAsync(List<Guid> partyUuids, bool includeSubunits = false, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpointUrl = $"parties/partylistbyuuid?fetchSubUnits={includeSubunits}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(partyUuids), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<List<Party>>(responseContent, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // GetPartiesAsync // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartiesAsync // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Party>> GetPartiesForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpointUrl = $"{_platformSettings.ApiAuthorizationEndpoint}parties?userId={userId}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl, token, accessToken, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<List<Party>>(responseContent, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // GetPartiesForUserAsync // Unexpected HttpStatusCode: {StatusCode}\n {responseContent}", response.StatusCode, responseContent);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetPartiesForUserAsync // Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<int>> GetKeyRoleParties(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/partieswithkeyroleaccess?userid={userId}");
            HttpResponseMessage response = await _client.GetAsync(uriBuilder.Uri, cancellationToken);
            string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<int>>(responseBody, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // GetKeyRoleParties // Failed // Unexpected HttpStatusCode: {StatusCode}\n {responseBody}", response.StatusCode, responseBody);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetKeyRoleParties // Failed // Unexpected Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<MainUnit>> GetMainUnits(MainUnitQuery subunitPartyIds, CancellationToken cancellationToken = default)
    {
        try
        {
            UriBuilder uriBuilder = new UriBuilder($"{_sblBridgeSettings.BaseApiUrl}authorization/api/partyparents");
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(subunitPartyIds), Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = uriBuilder.Uri,
                Content = requestBody
            };

            HttpResponseMessage response = await _client.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<List<MainUnit>>(responseBody, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // GetMainUnits // Failed // Unexpected HttpStatusCode: {StatusCode}\n {responseBody}", response.StatusCode, responseBody);
            return new();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // GetMainUnits // Failed // Unexpected Exception");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Party> LookupPartyBySSNOrOrgNo(PartyLookup partyLookup, CancellationToken cancellationToken = default)
    {
        try
        {
            string endpointUrl = $"parties/lookup";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _platformSettings.JwtCookieName);
            var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "access-management");
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(partyLookup, _serializerOptions), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(token, endpointUrl, requestBody, accessToken, cancellationToken);
            string responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return JsonSerializer.Deserialize<Party>(responseContent, _serializerOptions);
            }

            _logger.LogError("AccessManagement // PartiesClient // LookupPartyBySSNOrOrgNo // Unexpected HttpStatusCode: {StatusCode}\n {responseBody}", response.StatusCode, responseContent);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AccessManagement // PartiesClient // LookupPartyBySSNOrOrgNo // Exception");
            throw;
        }
    }
}
