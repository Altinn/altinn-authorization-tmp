﻿using Altinn.AccessManagement.Core.Models;
using Altinn.AccessManagement.Core.Models.ResourceRegistry;

namespace Altinn.AccessManagement.Core.Services.Interfaces
{
    /// <summary>
    /// Service for creating and updating Resources in AccessMAnagment existing in the Resource Registry
    /// </summary>
    public interface IResourceAdministrationPoint
    {
        /// <summary>
        /// Creates or Updates a list of Resources from Resource Registry
        /// </summary>
        /// <param name="resources">The resources to add or update</param>
        /// <returns></returns>
        Task<List<AccessManagementResource>> TryWriteResourceFromResourceRegistry(List<AccessManagementResource> resources);

        /// <summary>
        /// Gets a list of Resources from Resource Registry
        /// </summary>
        /// <param name="resourceType">The type of resource to be filtered</param>
        /// <returns>resource list based on resource type</returns>
        Task<List<ServiceResource>> GetResources(ResourceType resourceType);

        /// <summary>
        /// Gets a list of Resources from Resource Registry
        /// </summary>
        /// <param name="scope">The scope of the resource</param>
        /// <param name="cancellationToken"> Cancellation token to cancel the operation</param>
        /// <returns>resource list based on given scope</returns>
        Task<IEnumerable<ServiceResource>> GetResources(string scope, CancellationToken cancellationToken);

        /// <summary>
        /// Integration point for retrieving a single resoure by it's resource id
        /// </summary>
        /// <param name="resourceRegistryId">The identifier of the resource in the Resource Registry</param>
        /// <returns>The resource if exists</returns>
        Task<ServiceResource> GetResource(string resourceRegistryId);
    }
}
