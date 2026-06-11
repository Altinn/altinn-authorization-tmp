using System.Diagnostics;
using Altinn.AccessManagement.Core.Constants;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.ResourceRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.HostedServices.Services
{
    /// <inheritdoc />
    public partial class ResourceQueueSyncService(
        IAltinnResourceRegistry resourceRegistry,
        IServiceProvider serviceProvider,
        ILogger<ResourceQueueSyncService> logger) : IResourceQueueSyncService
    {
        private readonly ILogger<ResourceQueueSyncService> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IAltinnResourceRegistry _resourceRegistry = resourceRegistry;
        public async Task SyncResources(ILease lease, CancellationToken cancellationToken)
        {
            int elementsFetched = 0;
            
            do
            {
                var options = new AuditValues(SystemEntityConstants.ResourceRegistryImportSystem);
                using var scope = _serviceProvider.CreateEFScope(options);
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var resourceQueueService = scope.ServiceProvider.GetRequiredService<IResourceQueueService>();

                var leaseData = await lease.Get<ResourceQueueLease>(cancellationToken);
                List<ResourceQueue> resourcesToFetch = await resourceQueueService.RetrieveItemsForProcessing(leaseData.NextElementToFetch, cancellationToken);
                long lastFetchedId = resourcesToFetch.LastOrDefault()?.Id ?? 1;
                elementsFetched = resourcesToFetch.Count;

                foreach (var resource in resourcesToFetch)
                {
                    // Fetch resource from resource registry
                    var resourceResult = await UpsertResource(dbContext, resource.ResourceIdentifier, cancellationToken);

                    await UpsertResourceRules(dbContext, resource.ResourceIdentifier, resourceResult.Id, cancellationToken);
                }

                await lease.Update<ResourceQueueLease>(l => l.NextElementToFetch = lastFetchedId + 1, cancellationToken);
            } 
            while (elementsFetched == 100);
        }

        private async Task UpsertResourceRules(AppDbContext dbContext, string resourceIdentifier, Guid resourceId, CancellationToken cancellationToken)
        {
            // Fetch all packages and roles from the rulelist
            var packagesAndRoles = await FetchPackageAndRolesFromResourceRules(dbContext, resourceIdentifier, resourceId, cancellationToken);

            await SyncPackagesAndRoles(dbContext, resourceId, packagesAndRoles.RoleResources, packagesAndRoles.PackageResources, cancellationToken);
        }

        private async Task<(List<RoleResource> RoleResources, List<PackageResource> PackageResources)> FetchPackageAndRolesFromResourceRules(AppDbContext dbContext, string resourceIdentifier, Guid resourceId, CancellationToken cancellationToken)
        {
            // Fetch resource rules from resource registry
            var resourceRules = await _resourceRegistry.GetResourceRules(resourceIdentifier, cancellationToken);
            HashSet<string> roleNames = [];
            HashSet<string> packageNames = [];
            List<RoleResource> roleResources = [];
            List<PackageResource> packageResources = [];

            if (resourceRules.IsSuccessful)
            {
                foreach (var resourceRule in resourceRules.Content)
                {
                    bool isRole = resourceRule.Subject.All(r => r.Type.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.InvariantCultureIgnoreCase));                    
                    if (isRole)
                    {
                        string roleName = resourceRule.Subject.First(r => r.Type.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.RoleAttribute, StringComparison.InvariantCultureIgnoreCase)).Value;
                        if (roleNames.Contains(roleName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        else
                        {
                            var validRole = RoleConstants.TryGetByCode(roleName, out ConstantDefinition<Role> role);
                            if (validRole)
                            {
                                roleNames.Add(roleName);
                                roleResources.Add(new RoleResource()
                                {
                                    RoleId = role.Id,
                                    ResourceId = resourceId
                                });
                            }                            
                        }
                    }

                    bool isPackage = resourceRule.Subject.All(r => r.Type.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackageAttribute, StringComparison.InvariantCultureIgnoreCase));
                    if (isPackage)
                    {
                        string packageName = resourceRule.Subject.First(r => r.Type.Equals(AltinnXacmlConstants.MatchAttributeIdentifiers.AccessPackageAttribute, StringComparison.InvariantCultureIgnoreCase)).Value;
                        if (packageNames.Contains(packageName, StringComparer.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }
                        else
                        {
                            var validPackage = PackageConstants.TryGetByCode(packageName, out ConstantDefinition<Package> package);
                            if (validPackage)
                            {
                                packageNames.Add(packageName);
                                packageResources.Add(new PackageResource()
                                {
                                    PackageId = package.Id,
                                    ResourceId = resourceId
                                });
                            }
                        }
                    }
                }
            }
            else
            {
                Log.FailedToReadResourceRules(_logger);
            }

            return (roleResources, packageResources);
        }

        private async Task<Resource?> UpsertResource(AppDbContext dbContext, string resourceIdentifier, CancellationToken cancellationToken)
        {
            var fetchedResource = await _resourceRegistry.GetResource(resourceIdentifier, cancellationToken: cancellationToken);
            if (fetchedResource.IsProblem)
            {
                Log.FailedToGetResource(_logger, resourceIdentifier);
                return null;
            }

            var convertedResource = await ConvertToResource(dbContext, fetchedResource.Content, cancellationToken);
            if (convertedResource is null)
            {
                return null;
            }

            var resource = await dbContext.Resources.FirstOrDefaultAsync(t => t.RefId == convertedResource.RefId, cancellationToken);

            var rights = await FetchRolesAndPackages(resourceIdentifier, cancellationToken);

            if (resource is null)
            {
                dbContext.Resources.Add(convertedResource);

                //// TODO: add rights to resource here

                await dbContext.SaveChangesAsync(cancellationToken);
                return convertedResource;
            }

            resource.Name = convertedResource.Name;
            resource.Description = convertedResource.Description;
            resource.TypeId = convertedResource.TypeId;
            resource.ProviderId = convertedResource.ProviderId;

            //// TODO: update rights to resource here

            await dbContext.SaveChangesAsync(cancellationToken);

            return resource;
        }

        private async Task<(List<string> Roles, List<string> Packages)> FetchRolesAndPackages(string resourceIdentifier, CancellationToken cancellationToken)
        {
            List<string> roles = new List<string>();
            List<string> packages = new List<string>();

            //// TODO: implement fetching of roles and packages from resource registry when endpoint is available. For now, return empty lists.

            return (roles, packages);
        }

        public IEnumerable<ResourceType> ResourceTypes { get; set; }

        private async Task<Resource> ConvertToResource(AppDbContext dbContext, ResourceModel model, CancellationToken cancellationToken)
        {
            var provider = await dbContext.Providers
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.Code == model.HasCompetentAuthority.Orgcode.ToLowerInvariant(), cancellationToken);

            if (provider is null)
            {
                return null;
            }

            var resourceType = await GetOrCreateResourceType(dbContext, model, cancellationToken) ?? throw new InvalidOperationException("Unable to get or create resourcetype");
            return new Resource()
            {
                Name = model.Title?.Nb ?? model.Identifier,
                Description = model.Description?.Nb ?? "-",
                RefId = model.Identifier,
                ProviderId = provider.Id,
                TypeId = resourceType.Id
            };
        }

        private async Task LoadResourceTypes(AppDbContext dbContext, CancellationToken cancellationToken)
        {
            ResourceTypes = await dbContext.ResourceTypes.ToListAsync(cancellationToken);
        }

        private async Task<ResourceType> GetOrCreateResourceType(AppDbContext dbContext, ResourceModel model, CancellationToken cancellationToken)
        {
            if (ResourceTypes == null || !ResourceTypes.Any())
            {
                await LoadResourceTypes(dbContext, cancellationToken);
            }

            var type = ResourceTypes.FirstOrDefault(t => t.Name.Equals(model.ResourceType, StringComparison.OrdinalIgnoreCase));

            if (type == null)
            {
                type = new ResourceType()
                {
                    Id = Guid.CreateVersion7(),
                    Name = model.ResourceType
                };

                dbContext.ResourceTypes.Add(type);
                await dbContext.SaveChangesAsync(cancellationToken);

                await LoadResourceTypes(dbContext, cancellationToken);
            }

            return type;
        }

        private async Task SyncPackagesAndRoles(AppDbContext dbContext, Guid resourceId, List<RoleResource> fetchedRoles, List<PackageResource> fetchedPackages, CancellationToken cancellationToken)
        {
            // Sync PackageResources
            var existingPackages = await dbContext.PackageResources
                .Where(p => p.ResourceId == resourceId)
                .ToListAsync(cancellationToken);

            var packagesToRemove = existingPackages
                .Where(ep => !fetchedPackages.Any(fp => fp.PackageId == ep.PackageId && fp.ResourceId == ep.ResourceId))
                .ToList();

            var packagesToAdd = fetchedPackages
                .Where(fp => !existingPackages.Any(ep => ep.PackageId == fp.PackageId && ep.ResourceId == fp.ResourceId))
                .ToList();

            dbContext.PackageResources.RemoveRange(packagesToRemove); 
            dbContext.PackageResources.AddRange(packagesToAdd);

            // Sync RoleResources
            var existingRoles = await dbContext.RoleResources
                .Where(r => r.ResourceId == resourceId)
                .ToListAsync(cancellationToken);

            var rolesToRemove = existingRoles
                .Where(er => !fetchedRoles.Any(fr => fr.RoleId == er.RoleId && fr.ResourceId == er.ResourceId))
                .ToList();

            var rolesToAdd = fetchedRoles
                .Where(fr => !existingRoles.Any(er => er.RoleId == fr.RoleId && er.ResourceId == fr.ResourceId))
                .ToList();

            dbContext.RoleResources.RemoveRange(rolesToRemove);
            dbContext.RoleResources.AddRange(rolesToAdd);

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static partial class Log
        {
            [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unable to retrieve resource '{Resource}' from the resource registry.")]
            internal static partial void FailedToGetResource(ILogger logger, string resource);

            [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to read stream of updated resources from the resource registry.")]
            internal static partial void FailedToReadFromStream(ILogger logger);

            [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to retrieve list of packages and roles giving access to this resource.")]
            internal static partial void FailedToReadResourceRules(ILogger logger);

            [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "failed to write update subject {SubjectUrn} for resource {ResourceId} .")]
            internal static partial void FailedToWriteUpdateSubjectForResource(ILogger logger, Exception ex, string subjectUrn, string resourceId);
        }
    }
}
