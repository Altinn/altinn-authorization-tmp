using System.Security.Cryptography;
using System.Text;
using Altinn.AccessManagement.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.ResourceRegister;
using static Altinn.Authorization.AccessManagement.RegisterHostedService;

namespace Altinn.AccessManagement.HostedServices.Services;

/// <inheritdoc />
public class ResourceSyncService(
        IAltinnLease lease,
        IAltinnResourceRegister resourceRegister,
        IIngestService ingestService,
        IResourceTypeRepository resourceTypeRepository,
        IResourceRepository resourceRepository,
        IProviderRepository providerRepository,
        IProviderTypeRepository providerTypeRepository,
        ILogger<ResourceSyncService> logger) : IResourceSyncService
{
    private readonly IAltinnLease _lease = lease;
    private readonly IAltinnResourceRegister _resourceRegister = resourceRegister;
    private readonly IIngestService _ingestService =ingestService;
    private readonly IResourceTypeRepository _resourceTypeRepository = resourceTypeRepository;
    private readonly IResourceRepository _resourceRepository = resourceRepository;
    private readonly IProviderRepository _providerRepository = providerRepository;
    private readonly IProviderTypeRepository _providerTypeRepository = providerTypeRepository;
    private readonly ILogger _logger = logger;

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

        await foreach (var page in await _resourceRegister.StreamResources(ls.Data.ResourcesNextPageLink, cancellationToken))
        {
            if (!page.IsSuccessful)
            {
                Log.UpdatedResourceError(_logger, page.StatusCode);
                return;
            }

            var resources = new List<Resource>();

            foreach (var updatedResource in page.Content.Data)
            {
                var resourceResponse = await _resourceRegister.GetResource(updatedResource.ResourceUrn.Split(":").Last(), cancellationToken);
                if (!resourceResponse.IsSuccessful)
                {
                    continue;
                }

                var resource = resourceResponse.Content;

                var res = (await _resourceRepository.Get(t => t.RefId, resource.Identifier)).FirstOrDefault();

                if (updatedResource.Deleted)
                {
                    if (res != null)
                    {
                        await _resourceRepository.Delete(res.Id, options: options);
                    }

                    continue;
                }

                var provider = providers.FirstOrDefault(t => t.RefId == resource.HasCompetentAuthority.Orgcode);
                
                if (provider == null)
                {
                    if (resource.HasCompetentAuthority != null)
                    {
                        provider = new Provider()
                        {
                            Name = resource.HasCompetentAuthority.Name.Nb,
                            RefId = resource.HasCompetentAuthority.Orgcode
                        };
                        await _providerRepository.Create(provider, options: options);
                        await _providerRepository.CreateTranslation(
                            new Provider() { 
                                    Id = provider.Id, 
                                    Name = resource.HasCompetentAuthority.Name.En
                                },
                            "eng", 
                            options: options,
                            cancellationToken: cancellationToken
                            );
                        await _providerRepository.CreateTranslation(
                            new Provider()
                            {
                                Id = provider.Id,
                                Name = resource.HasCompetentAuthority.Name.Nn
                            },
                            "nno",
                            options: options,
                            cancellationToken: cancellationToken
                            );
                        providers.Add(provider);
                    }
                    else
                    {
                        continue;
                    }
                }

                var type = resourceTypes.FirstOrDefault(t => t.Name.Equals(resource.ResourceType, StringComparison.OrdinalIgnoreCase));
                if (type == null)
                {
                    type = new ResourceType(MD5.HashData(Encoding.UTF8.GetBytes(resource.ResourceType)))
                    {
                        Name = resource.ResourceType
                    };
                    resourceTypes.Add(type);
                    await _resourceTypeRepository.Create(type, options: options);
                }

                resources.Add(new Resource()
                {
                    Name = resource.Title.Nb,
                    Description = resource.Description.Nb,
                    ProviderId = provider.Id,
                    TypeId = type.Id,
                    RefId = resource.Identifier,
                });
            }

            await _ingestService.IngestAndMergeData(resources, options: options, ["RefId", "ProviderId"]);

            await UpdateLease(ls, data => data.ResourcesNextPageLink = page.Content.Links.Next, cancellationToken);
        }
    }

    private async Task UpdateLease(LeaseResult<LeaseContent> ls, Action<LeaseContent> configureLeaseContent, CancellationToken cancellationToken)
    {
        configureLeaseContent(ls.Data);
        await _lease.Put(ls, ls.Data, cancellationToken);
        await _lease.RefreshLease(ls, cancellationToken);
    }
}
