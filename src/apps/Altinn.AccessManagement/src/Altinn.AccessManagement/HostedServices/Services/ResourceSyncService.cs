using System.Linq.Expressions;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.AccessManagement.HostedServices;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;
using Altinn.Authorization.Integration.Platform.ResourceRegister;
using Microsoft.FeatureManagement;

namespace Altinn.AccessManagement.HostedServices.Services;

/// <inheritdoc />
public partial class ResourceSyncService : BaseSyncService, IResourceSyncService
{
    private readonly ILogger<ResourceSyncService> _logger;
    private readonly IAltinnResourceRegister _resourceRegister;
    private readonly IIngestService _ingestService;
    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderTypeRepository _providerTypeRepository;
    private readonly IPackageResourceRepository _packageResourceRepository;
    private readonly IPackageRepository _packageRepository;
    private readonly IRoleResourceRepository _roleResourceRepository;
    private readonly IRoleLookupRepository _roleLookupRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public ResourceSyncService(
        IAltinnLease lease,
        IFeatureManager featureManager,
        IAltinnRegister register,
        IAltinnResourceRegister resourceRegister,
        IIngestService ingestService,
        IResourceTypeRepository resourceTypeRepository,
        IResourceRepository resourceRepository,
        IProviderRepository providerRepository,
        IPackageRepository packageRepository,
        IRoleRepository roleRepository,
        IPackageResourceRepository packageResourceRepository,
        IRoleResourceRepository roleResourceRepository,
        IRoleLookupRepository roleLookupRepository,
        IProviderTypeRepository providerTypeRepository,
        ILogger<ResourceSyncService> logger
        ) : base(lease, featureManager, register)
    {
        _logger = logger;
        _resourceRegister = resourceRegister;
        _ingestService = ingestService;
        _roleResourceRepository = roleResourceRepository;
        _roleLookupRepository = roleLookupRepository;
        _resourceRepository = resourceRepository;
        _resourceTypeRepository = resourceTypeRepository;
        _packageResourceRepository = packageResourceRepository;
        _providerRepository = providerRepository;
        _providerTypeRepository = providerTypeRepository;
        _packageRepository = packageRepository;
    }

    /// <inheritdoc />
    public async Task<bool> SyncResourceOwners(CancellationToken cancellationToken)
    {
        var serviceOwners = await _resourceRegister.GetServiceOwners(cancellationToken);
        if (!serviceOwners.IsSuccessful)
        {
            Log.FailedToReadResourceOwners(_logger);
            return false;
        }

        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.ResourceRegisterImportSystem,
            ChangedBySystem = AuditDefaults.ResourceRegisterImportSystem,
        };

        var providerType = (await _providerTypeRepository.Get(t => t.Name, "Tjenesteeier")).FirstOrDefault();
        if (providerType == null)
        {
            throw new Exception("Provider type 'ServiceOwner' not found");
        }

        var resourceOwners = new List<Provider>();
        foreach (var serviceOwner in serviceOwners.Content.Orgs)
        {
            // Check if exists without Id => RefId/Code
            resourceOwners.Add(new Provider()
            {
                Id = serviceOwner.Value.Id,
                LogoUrl = serviceOwner.Value.Logo,
                Name = serviceOwner.Value.Name.Nb,
                RefId = serviceOwner.Value.Orgnr,
                Code = serviceOwner.Key,
                TypeId = providerType.Id,
            });
        }

        // IngestService will map in Id property and update properties not matchaed
        await _ingestService.IngestAndMergeData(resourceOwners, options: options, ["Id"], cancellationToken: cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task SyncResources(LeaseResult<ResourceRegisterLease> ls, CancellationToken cancellationToken)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.ResourceRegisterImportSystem,
            ChangedBySystem = AuditDefaults.ResourceRegisterImportSystem
        };

        Providers = [.. await _providerRepository.Get()];
        ResourceTypes = [.. await _resourceTypeRepository.Get()];

        await foreach (var page in await _resourceRegister.StreamResources(ls.Data?.ResourceNextPageLink, cancellationToken))
        {
            if (page.IsProblem)
            {
                Log.FailedToReadFromStream(_logger);
                return;
            }

            var resources = new List<Resource>();
            var failed = new List<Failed>();

            foreach (var updatedResource in page.Content.Data)
            {
                try
                {
                    var resource = await UpsertResource(updatedResource, options, cancellationToken);
                    if (updatedResource.Deleted)
                    {
                        await Flush();
                        await DeleteERRoleMap(updatedResource, resource, options, cancellationToken);
                        await DeleteAltinnRoleResource(updatedResource, resource, options, cancellationToken);
                        await DeleteAccessPackageMap(updatedResource, resource, options, cancellationToken);
                    }
                    else
                    {
                        await UpsertERRoleResource(updatedResource, resource, options, cancellationToken);
                        await UpsertAltinnRoleResource(updatedResource, resource, options, cancellationToken);
                        await UpsertAccessPackageMap(updatedResource, resource, options, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    var failure = new Failed
                    {
                        Exception = ex,
                        UpdatedModel = updatedResource,
                    };

                    failed.Add(failure);
                    if (failed.Count > 10)
                    {
                        /// FIX: We should break here, but waiting for role import. 
                        // break;
                    }
                }
            }

            foreach (var failure in failed)
            {
                Console.WriteLine(failure.UpdatedModel.ResourceUrn);
                Console.WriteLine(failure.Exception.Message);
            }

            await Flush();

            await UpdateLease(ls, data => data.ResourceNextPageLink = page.Content.Links.Next, cancellationToken);

            async Task Flush()
            {
                await _ingestService.IngestAndMergeData(resources, options: options, ["RefId", "ProviderId"], cancellationToken: cancellationToken);
                resources.Clear();
            }
        }
    }

    private async Task<bool> DeleteERRoleMap(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:rolecode:", StringComparison.OrdinalIgnoreCase))
        {
            var roleLookupFilter = _roleLookupRepository.CreateFilterBuilder()
                .Add(r => r.Key, "ERCode", FilterComparer.Like)
                .Add(r => r.Value, updatedResource.SubjectUrn.Split(":").Last(), FilterComparer.Like);

            var roleLookups = await _roleLookupRepository.GetExtended(roleLookupFilter, cancellationToken: cancellationToken);
            if (roleLookups.Single().Role is var role && role != null)
            {
                var filter = _roleResourceRepository.CreateFilterBuilder()
                    .Equal(f => f.ResourceId, resource.Id)
                    .Equal(f => f.RoleId, role.Id);

                await _roleResourceRepository.Delete(filter, options, cancellationToken: cancellationToken);
            }
        }

        return false;
    }

    private async Task<bool> DeleteAltinnRoleResource(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:role:", StringComparison.OrdinalIgnoreCase))
        {
            Log.MissingAltinnRoles(_logger, "delete");
        }

        return await Task.FromResult(false);
    }

    private async Task<bool> DeleteAccessPackageMap(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:accesspackage:", StringComparison.OrdinalIgnoreCase))
        {
            var packages = await _packageRepository.Get(r => r.Urn, updatedResource.SubjectUrn, cancellationToken: cancellationToken);
            if (packages.Single() is var package && package != null)
            {
                var filter = _packageResourceRepository.CreateFilterBuilder()
                    .Equal(f => f.ResourceId, resource.Id)
                    .Equal(f => f.PackageId, package.Id);

                await _packageResourceRepository.Delete(filter, options, cancellationToken: cancellationToken);
                return true;
            }
        }

        return false;
    }

    private async Task<bool> UpsertAccessPackageMap(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:accesspackage:", StringComparison.OrdinalIgnoreCase))
        {
            var packages = await _packageRepository.Get(r => r.Urn, updatedResource.SubjectUrn, cancellationToken: cancellationToken);
            if (packages.Single() is var package && package != null)
            {
                var packageResource = new PackageResource
                {
                    PackageId = package.Id,
                    ResourceId = resource.Id,
                };

                var cmpProps = new List<Expression<Func<PackageResource, object>>>()
                {
                    p => p.PackageId,
                    p => p.ResourceId,
                };

                var updateProps = new List<Expression<Func<PackageResource, object>>>()
                {
                    r => r.PackageId,
                    r => r.ResourceId,
                };

                await _packageResourceRepository.Upsert(packageResource, updateProps, cmpProps, options, cancellationToken: cancellationToken);
                return true;
            }
        }

        return false;
    }

    private async Task<bool> UpsertAltinnRoleResource(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:role:", StringComparison.OrdinalIgnoreCase))
        {
            Log.MissingAltinnRoles(_logger, "upsert");
        }

        return await Task.FromResult(false);
    }

    private async Task<bool> UpsertERRoleResource(ResourceUpdatedModel updatedResource, Resource resource, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (updatedResource.SubjectUrn.StartsWith("urn:altinn:rolecode:", StringComparison.OrdinalIgnoreCase))
        {
            var roleLookupFilter = _roleLookupRepository.CreateFilterBuilder()
                .Add(r => r.Key, "LegacyCode", FilterComparer.Like)
                .Add(r => r.Value, updatedResource.SubjectUrn.Split(":").Last(), FilterComparer.Like);

            var roleLookups = await _roleLookupRepository.GetExtended(roleLookupFilter, cancellationToken: cancellationToken);
            if (roleLookups.Single().Role is var role && role != null)
            {
                var roleResource = new RoleResource
                {
                    ResourceId = resource.Id,
                    RoleId = role.Id,
                };

                var cmpProps = new List<Expression<Func<RoleResource, object>>>()
                {
                    r => r.ResourceId,
                    r => r.RoleId,
                };

                var updateProps = new List<Expression<Func<RoleResource, object>>>()
                {
                    r => r.ResourceId,
                    r => r.RoleId,
                };

                await _roleResourceRepository.Upsert(roleResource, updateProps, cmpProps, options, cancellationToken: cancellationToken);
                return true;
            }
        }

        return false;
    }

    private async Task<Resource> UpsertResource(ResourceUpdatedModel resourceUpdated, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        var response = await _resourceRegister.GetResource(resourceUpdated.ResourceUrn.Split(":").Last(), cancellationToken: cancellationToken);
        if (response.IsProblem)
        {
            Log.FailedToGetResource(_logger, resourceUpdated.ResourceUrn);
            return null;
        }

        var repositoryResource = await ConvertToResource(response.Content, options, cancellationToken);

        var cmpProps = new List<Expression<Func<Resource, object>>>()
        {
            r => r.RefId,
            r => r.ProviderId,
        };

        var updateProps = new List<Expression<Func<Resource, object>>>()
        {
            r => r.TypeId,
            r => r.Name,
            r => r.Description,
        };

        await _resourceRepository.Upsert(repositoryResource, updateProps, cmpProps, options, cancellationToken);

        var resources = await _resourceRepository.Get(r => r.RefId, response.Content.Identifier, cancellationToken: cancellationToken);
        return resources.First();
    }

    public IEnumerable<Provider> Providers { get; set; }

    public IEnumerable<ResourceType> ResourceTypes { get; set; }

    private async Task<Resource> ConvertToResource(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        var provider = await GetOrCreateProvider(model, options, cancellationToken) ?? throw new Exception("Unable to get or create provider");
        var resourceType = await GetOrCreateResourceType(model, options, cancellationToken) ?? throw new Exception("Unable to get or create resourcetype");

        return new Resource()
        {
            Name = model.Title.Nb,
            Description = "-",
            RefId = model.Identifier,
            ProviderId = provider.Id,
            TypeId = resourceType.Id
        };
    }

    private async Task LoadResourceTypes()
    {
        ResourceTypes = await _resourceTypeRepository.Get();
    }

    private async Task<ResourceType> GetOrCreateResourceType(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (ResourceTypes == null || !ResourceTypes.Any())
        {
            await LoadResourceTypes();
        }

        var type = ResourceTypes.FirstOrDefault(t => t.Name.Equals(model.ResourceType, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            type = new ResourceType()
            {
                Id = Guid.CreateVersion7(),
                Name = model.ResourceType
            };

            await _resourceTypeRepository.Create(type, options: options);
            await LoadResourceTypes();
        }

        return type;
    }

    private async Task LoadProviders()
    {
        Providers = await _providerRepository.Get();
    }

    private async Task<Provider> GetOrCreateProvider(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        if (Providers == null || !Providers.Any())
        {
            await LoadProviders();
        }

        var orgNo = model.HasCompetentAuthority.Organization.ToString() ?? string.Empty;
        var orgCode = model.HasCompetentAuthority.Orgcode.ToString() ?? string.Empty;

        var provider = Providers.Where(t => !string.IsNullOrEmpty(t.RefId)).FirstOrDefault(t => t.RefId.Equals(orgNo, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            provider = Providers.Where(t => !string.IsNullOrEmpty(t.Code)).FirstOrDefault(t => t.Code.Equals(orgCode, StringComparison.OrdinalIgnoreCase));
        }

        if (provider == null)
        {
            if (model.HasCompetentAuthority != null)
            {
                var providerType = (await _providerTypeRepository.Get(t => t.Name, "Tjenesteeier")).FirstOrDefault();
                if (providerType == null)
                {
                    throw new Exception("Provider type 'ServiceOwner' not found");
                }

                provider = new Provider()
                {
                    Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.Nb,
                    RefId = model.HasCompetentAuthority.Organization,
                    Code = model.HasCompetentAuthority.Orgcode,
                    TypeId = providerType.Id,
                };
                
                await _providerRepository.Create(provider, options: options, cancellationToken: cancellationToken);

                //if (model.HasCompetentAuthority.Name != null && model.HasCompetentAuthority.Name.En != null)
                //{
                //    provider = new Provider()
                //    {
                //        Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.En,
                //        RefId = model.HasCompetentAuthority.Orgcode
                //    };

                //    await _providerRepository.CreateTranslation(provider, "eng", options: options, cancellationToken: cancellationToken);
                //}

                //if (model.HasCompetentAuthority.Name != null && model.HasCompetentAuthority.Name.Nn != null)
                //{
                //    provider = new Provider()
                //    {
                //        Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.Nn,
                //        RefId = model.HasCompetentAuthority.Orgcode
                //    };

                //    await _providerRepository.CreateTranslation(provider, "nno", options: options, cancellationToken: cancellationToken);
                //}

                await LoadProviders();
            }
        }

        return provider;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Warning, Message = "Cannot perform operation '{operation}' for Altinn roles as they have not yet been synced to the database.")]
        internal static partial void MissingAltinnRoles(ILogger logger, string operation);

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unable to retrieve resource '{resource}' from the resource registry.")]
        internal static partial void FailedToGetResource(ILogger logger, string resource);

        [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to read stream of updated resources from the resource registry.")]
        internal static partial void FailedToReadFromStream(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to retrieve list of service owners from the resource registry.")]
        internal static partial void FailedToReadResourceOwners(ILogger logger);
    }
}

internal class Failed
{
    /// <summary>
    /// Model from page
    /// </summary>
    internal ResourceUpdatedModel UpdatedModel { get; set; }

    /// <summary>
    /// Single resource from Registry
    /// </summary>
    internal ResourceModel RawResource { get; set; }

    /// <summary>
    /// Exception
    /// </summary>
    internal Exception Exception { get; set; }
}
