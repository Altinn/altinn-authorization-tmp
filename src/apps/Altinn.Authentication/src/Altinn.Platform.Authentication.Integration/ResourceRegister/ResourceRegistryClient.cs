using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Authentication.Integration.Configuration;
using Altinn.Platform.Authentication.Core.Models.ResourceRegistry;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Security.Policy;
using Altinn.Platform.Authentication.Core.Models.Rights;
using System.Net.Http.Json;

namespace Altinn.Platform.Authentication.Integration.ResourceRegister
{
    /// <summary>
    /// Client implementation for integration with the Resource Registry
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ResourceRegistryClient : IResourceRegistryClient
    {
        /// <summary>
        /// How long a resource fetched from the resource registry is served from memory before it is fetched again.
        /// Rights validation looks up every resource of a request, a change request and their approvals, so the same
        /// resource is typically requested several times within minutes. The window is kept short so a change in the
        /// registry, such as a resource turning non-delegable, is picked up quickly; a stale hit within the window is
        /// still caught by the delegation check in Access Management at approval.
        /// </summary>
        public static readonly TimeSpan ResourceCacheDuration = TimeSpan.FromMinutes(5);

        private const string ResourceCacheKeyPrefix = "ResourceRegistryClient:Resource:";

        private readonly HttpClient _httpClient;
        private readonly ILogger<IResourceRegistryClient> _logger;
        private readonly PlatformSettings _platformSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMemoryCache _memoryCache;
        private readonly JsonSerializerOptions _serializerOptions =
            new() { PropertyNameCaseInsensitive = true };

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryClient"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default httpclientfactory</param>
        /// <param name="logger">the logger</param>
        /// <param name="httpContextAccessor">handler for http context</param>
        /// <param name="platformSettings">the platform setttings</param>
        /// <param name="memoryCache">the memory cache that holds resources fetched from the registry for a short while</param>
        public ResourceRegistryClient(
            HttpClient httpClient,
            ILogger<ResourceRegistryClient> logger,
            IHttpContextAccessor httpContextAccessor,
            IOptions<PlatformSettings> platformSettings,
            IMemoryCache memoryCache)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _platformSettings = platformSettings.Value;
            httpClient.BaseAddress = new Uri(_platformSettings.ApiResourceRegistryEndpoint!);
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        /// <inheritdoc/>
        /// <remarks>
        /// A found resource is cached for <see cref="ResourceCacheDuration"/>. A miss (404 or other non-OK status)
        /// is not cached, so a resource that is created in the registry right after a lookup is seen on the next call.
        /// </remarks>
        public async Task<ServiceResource?> GetResource(string resourceId)
        {
            string cacheKey = ResourceCacheKeyPrefix + resourceId;
            if (_memoryCache.TryGetValue(cacheKey, out ServiceResource? cachedResource) && cachedResource is not null)
            {
                return cachedResource;
            }

            ServiceResource? resource = null;

            try
            {
                // It's not possible to filter on AltinnApp or Altinn2Service for this endpoint
                string endpointUrl = $"resource/{HttpUtility.UrlEncode(resourceId)}";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    JsonSerializerOptions options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    string content = await response.Content.ReadAsStringAsync();
                    resource = JsonSerializer.Deserialize<ServiceResource>(content, options);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication // ResourceRegistryClient // SearchResources // Exception");
                throw;
            }

            if (resource is not null)
            {
                _memoryCache.Set(cacheKey, resource, new MemoryCacheEntryOptions().SetAbsoluteExpiration(ResourceCacheDuration));
            }

            return resource;
        }

        public async Task<List<PolicyRightsDTO>> GetRights(string resourceId)
        {
            try
            {
                string endpointUrl = $"resource/{HttpUtility.UrlEncode(resourceId)}/policy/rights";

                HttpResponseMessage response = await _httpClient.GetAsync(endpointUrl);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var policyRightsDTOs = await response.Content.ReadFromJsonAsync<List<PolicyRightsDTO>>(_serializerOptions);
                    if (policyRightsDTOs is not null)
                    {
                        return policyRightsDTOs;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Authentication // ResourceRegistryClient // GetRights // Exception");
                throw;
            }
            return [];
        }
    }
}
