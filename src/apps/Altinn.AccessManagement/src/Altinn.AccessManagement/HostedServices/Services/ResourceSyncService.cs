using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
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
public class ResourceSyncService : BaseSyncService, IResourceSyncService
{
    private readonly ILogger<ResourceSyncService> _logger;
    private readonly IAltinnLease _lease;
    private readonly IAltinnResourceRegister _resourceRegister;
    private readonly IIngestService _ingestService;

    private readonly IResourceTypeRepository _resourceTypeRepository;
    private readonly IResourceRepository _resourceRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IProviderTypeRepository _providerTypeRepository;

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
        IProviderTypeRepository providerTypeRepository,
        ILogger<ResourceSyncService> logger
        ) : base(lease, featureManager, register)
    {
        _logger = logger;

        _resourceRegister = resourceRegister;
        _ingestService = ingestService;

        _resourceRepository = resourceRepository;
        _resourceTypeRepository = resourceTypeRepository;
        _providerRepository = providerRepository;
        _providerTypeRepository = providerTypeRepository;
    }

    /// <inheritdoc />
    public async Task<bool> SyncResourceOwners(CancellationToken cancellationToken)
    {
        var serviceOwners = await _resourceRegister.GetServiceOwners(cancellationToken);
        if (!serviceOwners.IsSuccessful)
        {
            Log.ServiceOwnerError(_logger, serviceOwners.StatusCode);
            return false;
        }

        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.RegisterImportSystem,
            ChangedBySystem = AuditDefaults.RegisterImportSystem
        };

        var providerType = (await _providerTypeRepository.Get(t => t.Name, "Tjenesteeier")).FirstOrDefault();
        if (providerType == null)
        {
            throw new Exception("Provider type 'ServiceOwner' not found");
        }

        var resourceOwners = new List<Provider>();
        foreach (var serviceOwner in serviceOwners.Content.Orgs)
        {
            resourceOwners.Add(new Provider()
            {
                Id = serviceOwner.Value.Id,
                LogoUrl = serviceOwner.Value.Logo,
                Name = serviceOwner.Value.Name.Nb,
                RefId = serviceOwner.Value.Orgnr,
                Code = serviceOwner.Key,
                TypeId = providerType.Id
            });
        }

        // IngestService will map in Id property and update properties not matchaed
        await _ingestService.IngestAndMergeData(resourceOwners, options: options, ["Id"]);

        return true;
    }

    /// <inheritdoc />
    public async Task SyncResources(LeaseResult<LeaseContent> ls, CancellationToken cancellationToken)
    {
        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.RegisterImportSystem,
            ChangedBySystem = AuditDefaults.RegisterImportSystem
        };

        var providers = (await _providerRepository.Get()).ToList();
        var resourceTypes = (await _resourceTypeRepository.Get()).ToList();

        await foreach (var page in await _resourceRegister.StreamResources(ls.Data?.ResourcesNextPageLink, cancellationToken))
        {
            if (!page.IsSuccessful)
            {
                Log.UpdatedResourceError(_logger, page.StatusCode);
                return;
            }

            var resources = new List<Resource>();
            var failed = new List<Failed>();

            foreach (var updatedResource in page.Content.Data)
            {
                if (updatedResource.Deleted)
                {
                    // Flush and delete from db
                    await Flush();

                    var filter = _resourceRepository.CreateFilterBuilder();
                    filter.Equal(t => t.RefId, updatedResource.ResourceUrn);
                    filter.Equal(t => t.ProviderId, Guid.Empty); // Get provider...
                    await _resourceRepository.Delete(filter, options: options, cancellationToken: cancellationToken);
                }

                var resourceResponse = await _resourceRegister.GetResource(updatedResource.ResourceUrn.Split(":").Last(), cancellationToken);
                if (!resourceResponse.IsSuccessful)
                {
                    throw new Exception(resourceResponse.StatusCode.ToString() + updatedResource.ResourceUrn.Split(":").Last());
                }

                try
                {
                    var resource = await ConvertToResource(resourceResponse.Content, options: options, cancellationToken: cancellationToken);

                    if (!resources.Any(t => t.Id.Equals(resource.Id)))
                    {
                        resources.Add(resource);
                    }
                }
                catch (Exception ex)
                {
                    var f = new Failed();
                    f.Exception = ex;
                    f.UpdatedModel = updatedResource;
                    f.RawResource = resourceResponse.Content;
                    failed.Add(f);

                    if (failed.Count > 10)
                    {
                        break;
                    }
                }
            }

            foreach (var f in failed)
            {
                Console.WriteLine(f.UpdatedModel.ResourceUrn);
                Console.WriteLine(f.Exception.Message);
            }

            await Flush();

            await UpdateLease(ls, data => data.ResourcesNextPageLink = page.Content.Links.Next, cancellationToken);

            async Task Flush()
            {
                await _ingestService.IngestAndMergeData(resources, options: options, ["RefId", "ProviderId"]);
                resources.Clear();
            }
        }
    }

    public List<Provider> Providers { get; set; }

    public List<ResourceType> ResourceTypes { get; set; }

    private async Task<Resource> ConvertToResource(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        var res = (await _resourceRepository.Get(t => t.RefId, model.Identifier)).FirstOrDefault();

        var provider = await GetOrCreateProvider(model, options, cancellationToken);
        var resourceType = await GetOrCreateResourceType(model, options, cancellationToken);

        return new Resource()
        {
            Id = res == null ? Guid.CreateVersion7() : res.Id,
            Name = model.Title.Nb,
            Description = model.Description?.Nb,
            RefId = model.Identifier,
            ProviderId = provider.Id,
            TypeId = resourceType.Id
        };
    }

    private async Task<ResourceType> GetOrCreateResourceType(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        var type = ResourceTypes.FirstOrDefault(t => t.Name.Equals(model.ResourceType, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            type = new ResourceType(MD5.HashData(Encoding.UTF8.GetBytes(model.ResourceType)))
            {
                Name = model.ResourceType
            };

            await _resourceTypeRepository.Create(type, options: options);
            ResourceTypes.Add(type);
        }

        return type;
    }

    private async Task<Provider> GetOrCreateProvider(ResourceModel model, ChangeRequestOptions options, CancellationToken cancellationToken)
    {
        var provider = Providers.FirstOrDefault(t => t.Code.Equals(model.HasCompetentAuthority.Orgcode, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            if (model.HasCompetentAuthority != null)
            {
                provider = new Provider()
                {
                    Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.Nb,
                    RefId = model.HasCompetentAuthority.Orgcode
                };
                await _providerRepository.Create(provider, options: options, cancellationToken: cancellationToken);

                if (model.HasCompetentAuthority.Name != null && model.HasCompetentAuthority.Name.En != null)
                {
                    provider = new Provider()
                    {
                        Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.En,
                        RefId = model.HasCompetentAuthority.Orgcode
                    };

                    await _providerRepository.CreateTranslation(provider, "eng", options: options, cancellationToken: cancellationToken);
                }

                if (model.HasCompetentAuthority.Name != null && model.HasCompetentAuthority.Name.Nn != null)
                {
                    provider = new Provider()
                    {
                        Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.Nn,
                        RefId = model.HasCompetentAuthority.Orgcode
                    };

                    await _providerRepository.CreateTranslation(provider, "nno", options: options, cancellationToken: cancellationToken);
                }

                Providers.Add(provider);
            }
        }

        return null;
    }
}

internal class Failed
{
    internal ResourceUpdatedModel UpdatedModel { get; set; }
    
    internal ResourceModel RawResource { get; set; }
    
    internal Exception Exception { get; set; }
}
