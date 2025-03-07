using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authorization.ABAC.Xacml;
using Altinn.Platform.Authorization.Configuration;
using Altinn.Platform.Authorization.Helpers;
using Altinn.Platform.Authorization.Models;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Services.Interface;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Platform.Authorization.Services.Implementation
{
    /// <summary>
    /// The Policy Retrieval point responsible to find the correct policy
    /// based on the context Request
    /// </summary>
    public class PolicyRetrievalPoint : IPolicyRetrievalPoint
    {
        private readonly IPolicyRepository _repository;
        private readonly IMemoryCache _memoryCache;
        private readonly GeneralSettings _generalSettings;
        private readonly IResourceRegistry _resourceRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolicyRetrievalPoint"/> class.
        /// </summary>
        /// <param name="policyRepository">The policy Repository..</param>
        /// <param name="memoryCache">The cache handler </param>
        /// <param name="settings">The app settings</param>
        /// <param name="resourceRegistry">The regis</param>
        public PolicyRetrievalPoint(IPolicyRepository policyRepository, IMemoryCache memoryCache, IOptions<GeneralSettings> settings, IResourceRegistry resourceRegistry)
        {
            _repository = policyRepository;
            _memoryCache = memoryCache;
            _generalSettings = settings.Value;
            _resourceRegistry = resourceRegistry;
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(XacmlContextRequest request)
        {
            PolicyResourceType policyResourceType = PolicyHelper.GetPolicyResourceType(request, out string resourceId, out string org, out string app);
            if (policyResourceType.Equals(PolicyResourceType.ResourceRegistry))
            {
                return await _resourceRegistry.GetResourcePolicyAsync(resourceId);
            }

            return await GetPolicyAsync(org, app);
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyAsync(string org, string app)
        {
            string policyPath = PolicyHelper.GetAltinnAppsPolicyPath(org, app);
            return await GetPolicyInternalAsync(policyPath);
        }

        /// <inheritdoc/>
        public async Task<XacmlPolicy> GetPolicyVersionAsync(string policyPath, string version, CancellationToken cancellationToken = default)
        {
            return await GetPolicyInternalAsync(policyPath, version, cancellationToken);
        }

        private async Task<XacmlPolicy> GetPolicyInternalAsync(string policyPath, string version = "", CancellationToken cancellationToken = default)
        {
            string cacheKey = policyPath + version;
            if (!_memoryCache.TryGetValue(cacheKey, out XacmlPolicy policy))
            {
                Stream policyBlob = string.IsNullOrEmpty(version) ?
                    await _repository.GetPolicyAsync(policyPath, cancellationToken) :
                    await _repository.GetPolicyVersionAsync(policyPath, version, cancellationToken);
                using (policyBlob)
                {
                    policy = (policyBlob.Length > 0) ? PolicyHelper.ParsePolicy(policyBlob) : null;
                }

                PutXacmlPolicyInCache(cacheKey, policy);
            }

            return policy;
        }

        private void PutXacmlPolicyInCache(string policyPath, XacmlPolicy policy)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
               .SetPriority(CacheItemPriority.High)
               .SetAbsoluteExpiration(new TimeSpan(0, _generalSettings.PolicyCacheTimeout, 0));

            _memoryCache.Set(policyPath, policy, cacheEntryOptions);
        }
    }
}
