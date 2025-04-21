using System.Diagnostics;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Integration.Platform.ResourceRegister;

namespace Altinn.AccessManagement.HostedServices.Jobs;

/// <inheritdoc />
public sealed class DbIngestResourceJob : DbIngestBase, IJob
{
    private readonly IAltinnResourceRegister ResourceRegister;

    private readonly IIngestService IngestService;

    private readonly IResourceTypeRepository ResourceTypeRepository;

    private readonly IResourceRepository ResourceRepository;

    private readonly IProviderRepository ProviderRepository;

    private readonly IProviderTypeRepository ProviderTypeRepository;

    /// <summary>
    /// Constructor
    /// </summary>
    public DbIngestResourceJob(
        IAltinnResourceRegister resourceRegister,
        IIngestService ingestService,
        IResourceTypeRepository resourceTypeRepository,
        IResourceRepository resourceRepository,
        IProviderRepository providerRepository,
        IProviderTypeRepository providerTypeRepository)
    {
        ResourceRegister = resourceRegister;
        IngestService = ingestService;
        ResourceRepository = resourceRepository;
        ResourceTypeRepository = resourceTypeRepository;
        ProviderRepository = providerRepository;
        ProviderTypeRepository = providerTypeRepository;
    }

    public async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        using var changeRequest = StartChangeRequest();
        var state = new StateManagement(ResourceTypeRepository, ProviderRepository);

        await RunResourceIngest(state, cancellationToken);
        return await RunResourceMapping(context, state, cancellationToken);
    }

    private async Task<JobResult> RunResourceIngest(StateManagement state, CancellationToken cancellationToken)
    {
        var activity = Activity.Current;
        var response = await ResourceRegister.GetResources(cancellationToken);

        if (!response.IsSuccessful)
        {
            return JobResult.Failure(response);
        }

        var ok = new List<Resource>();
        var failed = new List<ResourceModel>();
        foreach (var res in response.Content)
        {
            try
            {
                ok.Add(await ConvertToResource(res, state, cancellationToken));
            }
            catch (Exception ex)
            {
                activity?.AddException(ex);
                failed.Add(res);
            }
        }

        await IngestService.IngestAndMergeData(ok, CurrentChangeRequest, ["Id"], cancellationToken: cancellationToken);
        return JobResult.Success();
    }

    private async Task<JobResult> RunResourceMapping(JobContext context, StateManagement state, CancellationToken cancellationToken)
    {
        using var lease = await context.Lease.TryAquireNonBlocking<LeaseContent>("lease_name", cancellationToken);
        await foreach (var page in await ResourceRegister.StreamResources(lease.Data.NextPageLink, cancellationToken))
        {
            if (!lease.HasLease)
            {
                return JobResult.LostLease();
            }

            if (!page.IsSuccessful)
            {
                return JobResult.Failure();
            }

            var resources = new List<Resource>();
            var failed = new List<Failed>();
            foreach (var updatedResource in page.Content.Data)
            {
                if (updatedResource.Deleted)
                {
                    // Flush and delete from db
                    await Flush();

                    var filter = ResourceRepository.CreateFilterBuilder();
                    filter.Equal(t => t.RefId, updatedResource.ResourceUrn);
                    filter.Equal(t => t.ProviderId, Guid.Empty); // Get provider...
                    await ResourceRepository.Delete(filter, options: CurrentChangeRequest, cancellationToken: cancellationToken);
                }

                var resourceResponse = await ResourceRegister.GetResource(updatedResource.ResourceUrn.Split(":").Last(), cancellationToken);
                if (!resourceResponse.IsSuccessful)
                {
                    throw new Exception(resourceResponse.StatusCode.ToString() + updatedResource.ResourceUrn.Split(":").Last());
                }

                try
                {
                    var resource = await ConvertToResource(resourceResponse.Content, state, cancellationToken: cancellationToken);

                    if (!resources.Any(t => t.Id.Equals(resource.Id)))
                    {
                        resources.Add(resource);
                    }
                }
                catch (Exception ex)
                {
                    failed.Add(new Failed
                    {
                        Exception = ex,
                        UpdatedModel = updatedResource,
                        RawResource = resourceResponse.Content
                    });

                    if (failed.Count > 10)
                    {
                        break;
                    }
                }
            }

            foreach (var fail in failed)
            {
                Console.WriteLine(fail.UpdatedModel.ResourceUrn);
                Console.WriteLine(fail.Exception.Message);
            }

            await Flush();

            await UpsertAndRefreshLease(context.Lease, lease, data => data.NextPageLink = page.Content.Links.Next, cancellationToken);

            async Task Flush()
            {
                await IngestService.IngestAndMergeData(resources, options: CurrentChangeRequest, ["RefId", "ProviderId"]);
                resources.Clear();
            }
        }

        return JobResult.Success();
    }

    private async Task<Resource> ConvertToResource(ResourceModel model, StateManagement state, CancellationToken cancellationToken)
    {
        var provider = await GetOrCreateProvider(model, state, cancellationToken) ?? throw new Exception("Unable to get or create provider");
        var resourceType = await GetOrCreateResourceType(model, state, cancellationToken) ?? throw new Exception("Unable to get or create resourcetype");

        var filter = ResourceRepository.CreateFilterBuilder()
            .Equal(t => t.RefId, model.Identifier)
            .Equal(t => t.ProviderId, provider.Id);

        var res = (await ResourceRepository.Get(filter, cancellationToken: cancellationToken))
            .FirstOrDefault();

        if (string.IsNullOrEmpty(model?.Title?.Nb))
        {
            throw new Exception("Missing title");
        }

        return new Resource()
        {
            Id = res == null ? Guid.CreateVersion7() : res.Id,
            Name = model.Title.Nb,
            Description = "-",
            RefId = model.Identifier,
            ProviderId = provider.Id,
            TypeId = resourceType.Id
        };
    }

    private async Task<ResourceType> GetOrCreateResourceType(ResourceModel model, StateManagement state, CancellationToken cancellationToken)
    {
        if (state.ResourceTypes == null || !state.ResourceTypes.Any())
        {
            await state.RefreshResourceTypes(cancellationToken);
        }

        var type = state.ResourceTypes.FirstOrDefault(t => t.Name.Equals(model.ResourceType, StringComparison.OrdinalIgnoreCase));

        if (type == null)
        {
            type = new ResourceType()
            {
                Id = Guid.CreateVersion7(),
                Name = model.ResourceType
            };

            await ResourceTypeRepository.Create(type, CurrentChangeRequest, cancellationToken);
            await state.RefreshResourceTypes(cancellationToken);
        }

        return type;
    }

    private async Task<Provider> GetOrCreateProvider(ResourceModel model, StateManagement state, CancellationToken cancellationToken)
    {
        if (!state.Providers.Any())
        {
            await state.RefreshProviders(cancellationToken);
        }

        var providers = state.Providers;
        var orgNo = model.HasCompetentAuthority.Organization.ToString() ?? string.Empty;
        var orgCode = model.HasCompetentAuthority.Orgcode.ToString() ?? string.Empty;
        var provider = providers.Where(t => !string.IsNullOrEmpty(t.RefId)).FirstOrDefault(t => t.RefId.Equals(orgNo, StringComparison.OrdinalIgnoreCase));
        provider ??= providers.Where(t => !string.IsNullOrEmpty(t.Code)).FirstOrDefault(t => t.Code.Equals(orgCode, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            if (model.HasCompetentAuthority != null)
            {
                var providerType = (await ProviderTypeRepository.Get(t => t.Name, "Tjenesteeier"))
                    .FirstOrDefault();

                if (providerType == null)
                {
                    throw new Exception("Provider type 'ServiceOwner' not found");
                }

                provider = new Provider()
                {
                    Id = Guid.CreateVersion7(),
                    Name = model.HasCompetentAuthority.Name == null ? model.HasCompetentAuthority.Orgcode : model.HasCompetentAuthority.Name.Nb,
                    RefId = model.HasCompetentAuthority.Organization,
                    Code = model.HasCompetentAuthority.Orgcode,
                    TypeId = providerType.Id,
                };
                await ProviderRepository.Create(provider, CurrentChangeRequest, cancellationToken: cancellationToken);

                await state.RefreshProviders(cancellationToken);
            }
        }

        return provider;
    }

    internal class StateManagement(IResourceTypeRepository resourceTypeRepository, IProviderRepository providerRepository)
    {
        public IEnumerable<ResourceType> ResourceTypes => Volatile.Read(ref _resourceTypes) ?? [];

        public IEnumerable<Provider> Providers => Volatile.Read(ref _providers) ?? [];

        private IEnumerable<ResourceType> _resourceTypes = [];

        private IEnumerable<Provider> _providers = [];

        private IResourceTypeRepository ResourceTypeRepository { get; } = resourceTypeRepository;

        private IProviderRepository ProviderRepository { get; } = providerRepository;

        public async Task<IEnumerable<Provider>> RefreshProviders(CancellationToken cancellationToken)
        {
            var data = await ProviderRepository.Get(cancellationToken: cancellationToken);
            Interlocked.Exchange(ref _providers, data);
            return _providers;
        }

        public async Task<IEnumerable<ResourceType>> RefreshResourceTypes(CancellationToken cancellationToken)
        {
            var data = await ResourceTypeRepository.Get(cancellationToken: cancellationToken);
            Interlocked.Exchange(ref _resourceTypes, data);
            return _resourceTypes;
        }
    }

    public class LeaseContent
    {
        public string NextPageLink { get; set; }
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
