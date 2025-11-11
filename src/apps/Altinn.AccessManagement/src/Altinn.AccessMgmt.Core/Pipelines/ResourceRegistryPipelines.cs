using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Host.Pipeline.Services;
using Altinn.Authorization.Integration.Platform.ResourceRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Pipelines;

internal static class ResourceRegistryPipelines
{
    /// <summary>
    /// Service Owners Tasks
    /// </summary>
    internal static class ServiceOwnerTasks
    {
        /// <summary>
        /// Extracts all service owners from resource registry.
        /// </summary>
        /// <param name="context"><see cref="PipelineSourceContext"/></param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">is thrown if integration returns a problem.</exception>
        internal static async IAsyncEnumerable<IDictionary<string, ServiceOwner>> Extract(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var result = await resourceRegistry.GetServiceOwners(cancellationToken);
            var activity = Activity.Current;

            if (result.IsProblem)
            {
                throw new InvalidOperationException(result.ProblemDetails.Detail);
            }

            activity.SetTag("service_owner_count", result.Content.Orgs.Count);

            yield return result.Content.Orgs;
            yield break;
        }

        /// <summary>
        /// Transforms a <see cref="ServiceOwner"/> to an EF <see cref="Provider"/>
        /// </summary>
        /// <param name="context"><see cref="Transform(PipelineSegmentContext{IDictionary{string, ServiceOwner}})"/></param>
        internal static Task<List<Provider>> Transform(PipelineSegmentContext<IDictionary<string, ServiceOwner>> context)
        {
            var result = new List<Provider>();
            foreach (var serviceOwner in context.Data)
            {
                result.Add(new()
                {
                    Id = serviceOwner.Value.Id,
                    LogoUrl = serviceOwner.Value.Logo,
                    Name = serviceOwner.Value.Name.Nb,
                    RefId = serviceOwner.Value.Orgnr,
                    Code = serviceOwner.Key,
                    TypeId = ProviderTypeConstants.ServiceOwner,
                });
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Flushes all <see cref="Provider"/> to Database./>
        /// </summary>
        /// <param name="context">List of <see cref="Load(PipelineSinkContext{List{Provider}})"/></param>
        /// <returns></returns>
        internal static async Task Load(PipelineSinkContext<List<Provider>> context)
        {
            await PipelineUtils.Flush(context.Services, context.Data, ["id"]);
        }
    }

    /// <summary>
    /// Only writes resources with policies.
    /// </summary>
    internal static class ResourceTasks
    {
        private static Dictionary<string, Provider> CachedProviders { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, ResourceType> CachedResourcesTypes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        private static Dictionary<string, ResourceModel> CachedResources { get; set; } = [];

        private static readonly SemaphoreSlim _segmentLock = new(1, 1);

        private static readonly SemaphoreSlim _sourceLock = new(1, 1);

        public static async IAsyncEnumerable<(List<ResourceModel> Resources, string NextPage, DateTime UpdatedAt)> Extract(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var db = context.Services.ServiceProvider.GetRequiredService<AppDbContext>();
            var lease = await context.Lease.Get<Lease>(cancellationToken);
            var result = new List<ResourceModel>();

            await foreach (var page in await resourceRegistry.StreamResources(lease.UpdatedAt, lease.NextPage, cancellationToken))
            {
                PipelineUtils.EnsureSuccess(page);
                foreach (var updatedResource in page.Content.Data)
                {
                    var identifier = updatedResource.ResourceUrn.Split(':').Last();
                    if (CachedResources.TryGetValue(identifier, out var model))
                    {
                        result.Add(model);
                        continue;
                    }
                    else
                    {
                        await LoadResourceList();
                    }

                    if (CachedResources.TryGetValue(identifier, out model))
                    {
                        result.Add(model);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Couldn't find resource '{identifier}' in list of resources.");
                    }
                }

                yield return (
                    result.ToList(),
                    page.Content?.Links?.Next,
                    page.Content.Data.OrderByDescending(p => p.UpdatedAt).Select(p => p.UpdatedAt).FirstOrDefault()
                );

                result.Clear();
            }

            yield break;

            async Task LoadResourceList()
            {
                await _sourceLock.WaitAsync(cancellationToken);
                try
                {
                    var resources = await resourceRegistry.GetResources(cancellationToken);
                    if (resources.IsProblem)
                    {
                        throw new InvalidOperationException(resources.ProblemDetails.Title);
                    }

                    CachedResources = resources.Content.ToDictionary(r => r.Identifier);
                }
                finally
                {
                    _sourceLock.Release();
                }
            }
        }

        public static async Task<(List<Resource> Resources, string NextPage, DateTime UpdatedAt)> Transform(PipelineSegmentContext<(List<ResourceModel> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            Activity.Current?.SetTag("next_page", context.Data.NextPage);
            Activity.Current?.SetTag("updated_at", context.Data.UpdatedAt);

            var db = context.Services.ServiceProvider.GetRequiredService<AppDbContextFactory>().CreateDbContext();
            var result = new List<Resource>();

            foreach (var resource in context.Data.Resources)
            {
                try
                {
                    var (ok, provider) = await TryGetProvider(resource.HasCompetentAuthority.Orgcode);
                    if (!ok)
                    {
                        continue;
                    }

                    var resourceType = await UpsertResourceType(resource.ResourceType);

                    result.Add(new()
                    {
                        Name = resource.Title?.Nb ?? resource.Identifier,
                        Description = resource.Description?.Nb ?? "-",
                        RefId = resource.Identifier,
                        ProviderId = provider.Id,
                        TypeId = resourceType.Id
                    });
                }
                catch (InvalidOperationException ex)
                {
                    Activity.Current?.AddException(ex);
                    continue;
                }
            }

            result = result.DistinctBy(r => r.RefId).ToList();
            return (result, context.Data.NextPage, context.Data.UpdatedAt);

            async Task<(bool Ok, Provider? Value)> TryGetProvider(string code)
            {
                await _segmentLock.WaitAsync();
                try
                {
                    if (string.IsNullOrEmpty(code))
                    {
                        return (false, null);
                    }

                    if (CachedProviders.TryGetValue(code, out var provider))
                    {
                        return (true, provider);
                    }

                    CachedProviders = await db.Providers
                        .AsNoTracking()
                        .Where(r => !string.IsNullOrEmpty(r.Code))
                        .ToDictionaryAsync(r => r.Code, StringComparer.OrdinalIgnoreCase, CancellationToken.None);

                    if (CachedProviders.TryGetValue(code, out provider))
                    {
                        return (true, provider);
                    }

                    return (false, provider);
                }
                finally
                {
                    _segmentLock.Release();
                }
            }

            async Task<ResourceType> UpsertResourceType(string resourceTypeName)
            {
                await _segmentLock.WaitAsync();
                try
                {
                    if (CachedResourcesTypes.TryGetValue(resourceTypeName, out var resourceType))
                    {
                        return resourceType;
                    }

                    CachedResourcesTypes = await db.ResourceTypes
                        .AsNoTracking()
                        .ToDictionaryAsync(r => r.Name, CancellationToken.None);

                    if (CachedResourcesTypes.TryGetValue(resourceTypeName, out resourceType))
                    {
                        return resourceType;
                    }

                    if (CachedResourcesTypes.TryGetValue(resourceTypeName, out resourceType))
                    {
                        return resourceType;
                    }

                    resourceType = new ResourceType()
                    {
                        Name = resourceTypeName,
                    };

                    db.ResourceTypes.Add(resourceType);
                    await db.SaveChangesAsync();
                    return resourceType;
                }
                finally
                {
                    _segmentLock.Release();
                }
            }
        }

        public static async Task Load(PipelineSinkContext<(List<Resource> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            var merged = await PipelineUtils.Flush(context.Services, context.Data.Resources, ["refid"]);
            if (merged > 0)
            {
                await context.Lease.Update(new Lease()
                {
                    NextPage = context.Data.NextPage,
                    UpdatedAt = context.Data.UpdatedAt,
                });
            }
        }

        internal class Lease
        {
            public DateTime UpdatedAt { get; set; } = default;

            public string NextPage { get; set; }
        }
    }

    /// <summary>
    /// Contains a task for extracting <see cref="ResourceUpdatedModel"/> from resource registry.  
    /// </summary>
    internal static class UpdatedResourceTasks
    {
        /// <summary>
        /// Extracting <see cref="ResourceUpdatedModel"/> from resource registry.  
        /// </summary>
        internal static async IAsyncEnumerable<(List<ResourceUpdatedModel> Resources, string NextPage, DateTime UpdatedAt)> Extract(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var lease = await context.Lease.Get<Lease>(cancellationToken);

            await foreach (var page in await resourceRegistry.StreamResources(lease.UpdatedAt, lease.NextPage, cancellationToken))
            {
                PipelineUtils.EnsureSuccess(page);
                yield return (
                    page.Content.Data.ToList(),
                    page.Content?.Links?.Next,
                    page.Content.Data.OrderByDescending(p => p.UpdatedAt).Select(p => p.UpdatedAt).FirstOrDefault()
                );
            }

            yield break;
        }

        internal class Lease
        {
            public DateTime UpdatedAt { get; set; } = default;

            public string NextPage { get; set; }
        }
    }

    /// <summary>
    /// Contains tasks for transforming <see cref="ResourceUpdatedModel"/> to <see cref="RoleResource"/> 
    /// and to ingest list of <see cref="RoleResource"/>.
    /// </summary>
    internal static class RoleResourceTasks
    {
        private static Dictionary<string, Resource> CachedResources { get; set; } = [];

        private static Dictionary<string, RoleLookup> CachedRoleLookups { get; set; } = [];

        private static readonly SemaphoreSlim _resourceLock = new(1, 1);

        private static readonly SemaphoreSlim _roleLookupLock = new(1, 1);

        /// <summary>
        /// Transforms <see cref="ResourceUpdatedModel"/> to <see cref="RoleResource"/> 
        /// </summary>
        internal static async Task<(List<RoleResourceOperation> Resources, string NextPage, DateTime UpdatedAt)> Transform(PipelineSegmentContext<(List<ResourceUpdatedModel> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            Activity.Current?.SetTag("next_page", context.Data.NextPage);
            Activity.Current?.SetTag("updated_at", context.Data.UpdatedAt);

            using var db = context.Services.ServiceProvider.GetRequiredService<AppDbContextFactory>().CreateDbContext();
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var result = new List<RoleResourceOperation>();

            foreach (var updatedResource in context.Data.Resources)
            {
                if (updatedResource.SubjectUrn.StartsWith("urn:altinn:rolecode:"))
                {
                    var (roleOk, role) = await TryGetRoleLookup(updatedResource.SubjectUrn);
                    if (!roleOk)
                    {
                        continue;
                    }

                    var (resourceOk, resource) = await TryGetResource(updatedResource.ResourceUrn);
                    if (!resourceOk)
                    {
                        continue;
                    }

                    result.Add(new RoleResourceOperation()
                    {
                        Deleted = updatedResource.Deleted,
                        ResourceId = resource.Id,
                        RoleId = role.RoleId,
                    });
                }
            }

            return (result, context.Data.NextPage, context.Data.UpdatedAt);

            async Task<(bool Ok, RoleLookup RoleLookup)> TryGetRoleLookup(string urn)
            {
                await _roleLookupLock.WaitAsync();
                try
                {
                    if (string.IsNullOrEmpty(urn))
                    {
                        return (false, null);
                    }

                    var refId = urn.Split(":").Last();
                    if (CachedRoleLookups.TryGetValue(refId, out var roleLookup))
                    {
                        return (true, roleLookup);
                    }

                    CachedRoleLookups = await db.RoleLookups
                        .Where(r => r.Key == "LegacyCode")
                        .ToDictionaryAsync(r => r.Value, StringComparer.OrdinalIgnoreCase);

                    if (CachedRoleLookups.TryGetValue(refId, out roleLookup))
                    {
                        return (true, roleLookup);
                    }

                    Activity.Current?.AddEvent(new($"Couldn't find role in role lookup.", tags: [new("role", urn)]));
                    return (false, null);
                }
                finally
                {
                    _roleLookupLock.Release();
                }
            }

            async Task<(bool Ok, Resource? Resource)> TryGetResource(string urn)
            {
                await _resourceLock.WaitAsync();
                try
                {
                    var refId = urn.Split(":").Last();
                    if (CachedResources.TryGetValue(refId, out var resource))
                    {
                        return (true, resource);
                    }

                    // Reload cache
                    CachedResources = await db.Resources
                        .AsNoTracking()
                        .Where(r => !string.IsNullOrEmpty(r.RefId))
                        .ToDictionaryAsync(r => r.RefId, CancellationToken.None);

                    // Try get from cache
                    if (CachedResources.TryGetValue(refId, out resource))
                    {
                        return (true, resource);
                    }

                    // Check if it's another malformed resource.
                    var resourceLookup = await resourceRegistry.GetResource(refId);
                    if (resourceLookup.StatusCode == HttpStatusCode.NotFound)
                    {
                        Activity.Current?.AddEvent(new($"Couldn't find resource from Resource Registry.", tags: [new("resource_refid", refId)]));
                        return (false, null);
                    }

                    if (resourceLookup.IsProblem)
                    {
                        throw new InvalidOperationException(resourceLookup.ProblemDetails.Detail);
                    }

                    // Resource without org code? 
                    if (string.IsNullOrEmpty(resourceLookup.Content.HasCompetentAuthority.Orgcode))
                    {
                        Activity.Current?.AddEvent(new($"Found resource without orgcode.", tags: [new("resource_refid", refId)]));
                        return (false, null);
                    }

                    // org code is defined, check if service owner exists?
                    var provider = await db.Providers.FirstOrDefaultAsync(r => EF.Functions.ILike(r.Code, resourceLookup.Content.HasCompetentAuthority.Orgcode));
                    if (provider is null)
                    {
                        Activity.Current?.AddEvent(new($"Found resource, but service owner is deleted.", tags: [new("resource_refid", refId), new("provider_code", resourceLookup.Content.HasCompetentAuthority.Orgcode)]));
                        return (false, null);
                    }

                    throw new InvalidOperationException($"Resource should maybe exists, but it hasn't been created for an unknown reason.");
                }
                finally
                {
                    _resourceLock.Release();
                }
            }
        }

        /// <summary>
        /// Batch upserting a list of <see cref="RoleResourceOperation"/>. 
        /// </summary>
        internal static async Task Load(PipelineSinkContext<(List<RoleResourceOperation> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            var db = context.Services.ServiceProvider.GetRequiredService<AppDbContextFactory>().CreateDbContext();

            var add = new List<RoleResource>();
            var remove = new List<RoleResource>();
            var seen = new HashSet<(Guid, Guid)>();

            foreach (var roleResource in context.Data.Resources)
            {
                if (!seen.Add((roleResource.RoleId, roleResource.ResourceId)))
                {
                    await Flush();
                    seen.Add((roleResource.RoleId, roleResource.ResourceId));
                }

                if (roleResource.Deleted)
                {
                    remove.Add(roleResource);
                }
                else
                {
                    add.Add(roleResource);
                }
            }

            var updates = await Flush();
            if (updates > 0)
            {
                await context.Lease.Update(new Lease()
                {
                    NextPage = context.Data.NextPage,
                    UpdatedAt = context.Data.UpdatedAt,
                });
            }

            async Task<int> Flush()
            {
                var result = await Task.WhenAll(
                    Task.Run(async () => await PipelineUtils.Flush(context.Services, add, ["roleid", "resourceid"])),
                    Task.Run(async () =>
                    {
                        if (remove.Count > 0)
                        {
                            var roleIds = remove.Select(p => p.RoleId);
                            var result = await db.RoleResources
                                .AsTracking()
                                .Where(r => roleIds.Contains(r.RoleId))
                                .ToListAsync();

                            result = result
                                .Where(r => remove.Any(p => p.RoleId == r.RoleId && p.ResourceId == p.ResourceId))
                                .ToList();

                            db.RoleResources.RemoveRange(result);
                            return await db.SaveChangesAsync();
                        }

                        return 0;
                    })
                );

                seen.Clear();
                add.Clear();
                remove.Clear();

                return result.Sum();
            }
        }

        internal class RoleResourceOperation : RoleResource
        {
            public bool Deleted { get; set; }
        }

        internal class Lease
        {
            public DateTime UpdatedAt { get; set; } = default;

            public string NextPage { get; set; }
        }
    }

    /// <summary>
    /// Contains tasks for transforming <see cref="ResourceUpdatedModel"/> to <see cref="PackageResource"/> 
    /// and to ingest list of <see cref="PackageResource"/>.
    /// </summary>
    internal static class PackageResourceTasks
    {
        internal const string LeaseName = "resource_registry_pipeline_package_resource";

        private static Dictionary<string, Resource> CachedResources { get; set; } = [];

        private static readonly SemaphoreSlim _resourceLock = new(1, 1);

        internal static async Task<(List<PackageResourceOperation> Resources, string NextPage, DateTime UpdatedAt)> Transform(PipelineSegmentContext<(List<ResourceUpdatedModel> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            Activity.Current?.SetTag("next_page", context.Data.NextPage);
            Activity.Current?.SetTag("updated_at", context.Data.UpdatedAt);

            var db = context.Services.ServiceProvider.GetRequiredService<AppDbContextFactory>().CreateDbContext();
            var resourceRegistry = context.Services.ServiceProvider.GetRequiredService<IAltinnResourceRegistry>();
            var result = new List<PackageResourceOperation>();

            foreach (var updatedResource in context.Data.Resources)
            {
                if (updatedResource.SubjectUrn.StartsWith("urn:altinn:accesspackage:"))
                {
                    if (!PackageConstants.TryGetByUrn(updatedResource.SubjectUrn, out var pkg))
                    {
                        continue;
                    }

                    var (ok, resource) = await TryGetResource(updatedResource.ResourceUrn);
                    if (!ok)
                    {
                        continue;
                    }

                    result.Add(new PackageResourceOperation()
                    {
                        Deleted = updatedResource.Deleted,
                        ResourceId = resource.Id,
                        PackageId = pkg,
                    });
                }
            }

            return (result, context.Data.NextPage, context.Data.UpdatedAt);

            async Task<(bool Ok, Resource? Resource)> TryGetResource(string urn)
            {
                await _resourceLock.WaitAsync();
                try
                {
                    var refId = urn.Split(":").Last();
                    if (CachedResources.TryGetValue(refId, out var resource))
                    {
                        return (true, resource);
                    }

                    // Reload cache
                    CachedResources = await db.Resources
                        .AsNoTracking()
                        .Where(r => !string.IsNullOrEmpty(r.RefId))
                        .ToDictionaryAsync(r => r.RefId, CancellationToken.None);

                    // Try get from cache
                    if (CachedResources.TryGetValue(refId, out resource))
                    {
                        return (true, resource);
                    }

                    // Check if it's another malformed resource.
                    var resourceLookup = await resourceRegistry.GetResource(refId);
                    if (resourceLookup.StatusCode == HttpStatusCode.NotFound)
                    {
                        Activity.Current?.AddEvent(new($"Couldn't find resource from Resource Registry.", tags: [new("resource_refid", refId)]));
                        return (false, null);
                    }

                    if (resourceLookup.IsProblem)
                    {
                        throw new InvalidOperationException(resourceLookup.ProblemDetails.Detail);
                    }

                    // Resource without org code? 
                    if (string.IsNullOrEmpty(resourceLookup.Content.HasCompetentAuthority.Orgcode))
                    {
                        Activity.Current?.AddEvent(new($"Found resource without orgcode.", tags: [new("resource_refid", refId)]));
                        return (false, null);
                    }

                    throw new InvalidOperationException($"Unexpected result trying to get resources.");
                }
                finally
                {
                    _resourceLock.Release();
                }
            }
        }

        internal static async Task Load(PipelineSinkContext<(List<PackageResourceOperation> Resources, string NextPage, DateTime UpdatedAt)> context)
        {
            Activity.Current?.SetTag("next_page", context.Data.NextPage);
            Activity.Current?.SetTag("updated_at", context.Data.UpdatedAt);

            var db = context.Services.ServiceProvider.GetRequiredService<AppDbContextFactory>().CreateDbContext();

            var add = new List<PackageResource>();
            var remove = new List<PackageResource>();
            var seen = new HashSet<(Guid, Guid)>();

            foreach (var packageResource in context.Data.Resources)
            {
                if (!seen.Add((packageResource.PackageId, packageResource.ResourceId)))
                {
                    await Flush();
                    seen.Add((packageResource.PackageId, packageResource.ResourceId));
                }

                if (packageResource.Deleted)
                {
                    remove.Add(packageResource);
                }
                else
                {
                    add.Add(packageResource);
                }
            }

            await Flush();
            await context.Lease.Update(new Lease()
            {
                NextPage = context.Data.NextPage,
                UpdatedAt = context.Data.UpdatedAt,
            });

            async Task Flush()
            {
                await Task.WhenAll(
                    Task.Run(async () => await PipelineUtils.Flush(context.Services, add, ["packageid", "resourceid"])),
                    Task.Run(async () =>
                    {
                        if (remove.Count > 0)
                        {
                            var packageIds = remove.Select(p => p.PackageId);
                            var result = await db.PackageResources
                                .AsTracking()
                                .Where(r => packageIds.Contains(r.PackageId))
                                .ToListAsync();

                            result = result
                                .Where(r => remove.Any(p => p.PackageId == r.PackageId && p.ResourceId == p.ResourceId))
                                .ToList();

                            db.PackageResources.RemoveRange(result);
                            await db.SaveChangesAsync();
                        }
                    })
                );

                seen.Clear();
                add.Clear();
                remove.Clear();
            }
        }

        internal class Lease
        {
            public DateTime UpdatedAt { get; set; } = default;

            public string NextPage { get; set; }
        }

        internal class PackageResourceOperation : PackageResource
        {
            public bool Deleted { get; set; }
        }
    }
}
