using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services.Contracts;
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
        services.AddScoped<RegisterHostedService>();
        services.AddScoped<IIngestService, IngestService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IPackageService, PackageService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAssignmentService, AssignmentService>();
        services.AddScoped<IDelegationService, DelegationService>();
        services.AddScoped<IResourceService, ResourceService>();
        services.AddScoped<IEntityService, EntityService>();
        services.AddScoped<IAuthorizedPartyRepoService, AuthorizedPartyRepoService>();

        services.AddScoped<IAmPartyRepository, AMPartyService>();
        services.AddScoped<IEntityService, EntityService>();

        AddJobs(services);
        
        services.AddResourceRegistryPipeline();
        return services;
    }

    private static void AddResourceRegistryPipeline(this IServiceCollection services)
    {
        services.AddPipelinesOtel();
        services.AddPipelines(descriptor =>
        {
            descriptor
                // .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesResourceRegistrySync)
                .WithGroupName("Resource Registry Import")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Extract Service Owners")
                    .WithLease(ResourceRegistryPipelines.ServiceOwnerJobs.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.ServiceOwnerJobs.Extract)
                        .AddSegment("Transform", ResourceRegistryPipelines.ServiceOwnerJobs.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.ServiceOwnerJobs.Load)
                        .Build()
                .AddPipeline("Extract Resources")
                    .WithLease(ResourceRegistryPipelines.ResourceJobs.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.ResourceJobs.Extract)
                        .AddSegment("Transform", ResourceRegistryPipelines.ResourceJobs.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.ResourceJobs.Load)
                        .Build()
                .AddPipeline("Extract Package Resources")
                    .WithLease(ResourceRegistryPipelines.PackageResourceJobs.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.UpdatedResourceJobs.Extract)
                        .AddSegment("Transform", ResourceRegistryPipelines.PackageResourceJobs.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.PackageResourceJobs.Load)
                        .Build()
                .AddPipeline("Extract Role Resources")
                    .WithLease(ResourceRegistryPipelines.RoleResourceJobs.LeaseName)
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource("Extract", ResourceRegistryPipelines.UpdatedResourceJobs.Extract)
                        .AddSegment("Transform", ResourceRegistryPipelines.RoleResourceJobs.Transform)
                        .AddSink("Load", ResourceRegistryPipelines.RoleResourceJobs.Load)
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
