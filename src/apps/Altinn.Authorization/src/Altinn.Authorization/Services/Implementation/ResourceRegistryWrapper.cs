using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Authorization.Clients;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Helpers;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.Platform.Authorization.Services.Implementation
{
    /// <summary>
    /// Wrapper for resource registry
    /// </summary>
    public class ResourceRegistryWrapper : IResourceRegistry
    {
        private readonly ResourceRegistryClient _client;
        private readonly IMemoryCache _memoryCache;
        private readonly GeneralSettings _generalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRegistryWrapper"/> class.
        /// </summary>
        public ResourceRegistryWrapper(ResourceRegistryClient resourceRegistryClient, IMemoryCache memoryCache, IOptions<GeneralSettings> settings)
        {
            _client = resourceRegistryClient;
            _generalSettings = settings.Value;
            _memoryCache = memoryCache;
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetResourcePolicyAsync(string resourceId)
        {
            string cacheKey = "resourcepolicy:" + resourceId;
            if (!_memoryCache.TryGetValue(cacheKey, out XacmlPolicy policy))
            {
                string apiurl = $"resource/{resourceId}/policy";
                HttpResponseMessage response = await _client.Client.GetAsync(apiurl);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Stream policyBlob = await response.Content.ReadAsStreamAsync();
                    using (policyBlob)
                    {
                        policy = (policyBlob.Length > 0) ? PolicyHelper.ParsePolicy(policyBlob) : null;
                    }

                    PutXacmlPolicyInCache(cacheKey, policy);
                }
            }

            return policy;
        }

        private void PutXacmlPolicyInCache(string cachekey, XacmlPolicy policy)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.PolicyCacheTimeout, 0));

            _memoryCache.Set(cachekey, policy, cacheEntryOptions);
        }
    }
}
