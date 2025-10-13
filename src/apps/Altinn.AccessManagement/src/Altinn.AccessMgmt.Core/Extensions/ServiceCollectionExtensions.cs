using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessMgmt.Core.HostedServices;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Altinn.AccessMgmt.Core.Pipelines;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Pipeline.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessMgmtCore(this IServiceCollection services)
    {
        services.AddHostedService<RegisterHostedService>();
        services.AddScoped<IIngestService, IngestService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IDelegationService, DelegationService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IEntityService, EntityService>();

        services.AddScoped<IAmPartyRepository, AMPartyService>();
        services.AddScoped<IEntityService, EntityService>();

        AddJobs(services);
        return services;
    }

    private static void AddPipelines(IServiceCollection services)
    {
        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesResourceRegistrySync)
                .WithGroupName("Resource Registry Import")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Extract Service Owners")
                    .WithLease(ResourceRegistryPipelines.ServiceOwners.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.ServiceOwners.Stream)
                        .AddSegment("Transform", ResourceRegistryPipelines.ServiceOwners.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.ServiceOwners.Load)
                        .Build()
                .AddPipeline("Extract Resources")
                    .WithLease(ResourceRegistryPipelines.Resources.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.Resources.Stream)
                        .AddSegment("Transform", ResourceRegistryPipelines.Resources.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.Resources.Load)
                        .Build();
        });
    }

    private static void AddJobs(IServiceCollection services)
    {
        services.AddSingleton<IPartySyncService, PartySyncService>();
        services.AddSingleton<IRoleSyncService, RoleSyncService>();
        services.AddSingleton<IResourceSyncService, ResourceSyncService>();
    }
}
