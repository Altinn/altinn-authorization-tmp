using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Integration.Platform.ResourceRegister;

namespace Altinn.AccessManagement.HostedServices.Jobs;

/// <summary>
/// 
/// </summary>
public class DbIngestResourceOwners(
    IAltinnResourceRegister resourceRegister,
    IIngestService ingestService,
    IProviderTypeRepository providerTypeRepository
) : DbIngestBase, IJob
{
    private IAltinnResourceRegister ResourceRegister { get; } = resourceRegister;

    private IIngestService IngestService { get; } = ingestService;

    private IProviderTypeRepository ProviderTypeRepository { get; } = providerTypeRepository;

    /// <summary>
    /// kake
    /// </summary>
    public async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        var serviceOwners = await ResourceRegister.GetServiceOwners(cancellationToken);
        if (!serviceOwners.IsSuccessful)
        {
            return JobResult.Failure();
        }

        var providerType = (await ProviderTypeRepository.Get(t => t.Name, "Tjenesteeier", cancellationToken: cancellationToken)).FirstOrDefault();
        if (providerType == null)
        {
            return JobResult.Failure("failed to find ProviderType 'ServiceOwner' in database.");
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
                TypeId = providerType.Id
            });
        }

        // IngestService will map in Id property and update properties not matchaed
        using var changeRequest = StartChangeRequest();
        await IngestService.IngestAndMergeData(resourceOwners, options: changeRequest, ["Id"], cancellationToken: cancellationToken);

        return JobResult.Success();
    }
}
