using System.Diagnostics;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Integration.Platform.ResourceRegistry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.HostedServices;

public class ResourceRegistryServiceOwnerJob(
    IAltinnResourceRegistry ResourceRegistry,
    IIngestService IngestService
) : JobBase
{
    public override async Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        var featureManager = context.ServiceProvider.GetRequiredService<IFeatureManager>();
        return !await featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesResourceRegistrySync);
    }

    public override async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        using var lease = await context.Lease.TryAquireNonBlocking<LeaseContent>("access_management_resource_registry_service_owner", cancellationToken);
        if (!lease.HasLease)
        {
            return JobResult.CouldNotRun("Failed to aquire lease initially");
        }

        var dbContext = context.ServiceProvider.GetRequiredService<AppDbContext>();

        var serviceOwner = await dbContext.Providers.FirstOrDefaultAsync(t => t.Name == "Tjenesteeier", cancellationToken);
        if (serviceOwner is null)
        {
            return JobResult.Failure("Provider type 'Tjenesteeier' not found");
        }

        var serviceOwners = await ResourceRegistry.GetServiceOwners(cancellationToken);
        if (serviceOwners.IsProblem)
        {
            return JobResult.Failure(serviceOwners.ProblemDetails);
        }

        var resourceOwners = new List<Provider>();
        foreach (var org in serviceOwners.Content.Orgs)
        {
            resourceOwners.Add(new Provider()
            {
                Id = org.Value.Id,
                LogoUrl = org.Value.Logo,
                Name = org.Value.Name.Nb,
                RefId = org.Value.Orgnr,
                Code = org.Key,
                TypeId = serviceOwner.Id,
            });
        }

        var upsertedRows = IngestService.IngestAndMergeData(resourceOwners, NewAudit(), ["code"], cancellationToken);
        return JobResult.Success("Done merging resources.");
    }

    private AuditValues NewAudit()
    {
        var operationId = Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString();
        return new AuditValues(
            Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B"),
            Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B"),
            operationId
        );
    }

    internal class LeaseContent { }
}
