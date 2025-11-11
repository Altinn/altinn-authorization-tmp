using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessManagement.Core.Services;
using Altinn.AccessManagement.Core.Services.Contracts;
using Altinn.AccessManagement.Core.Services.Interfaces;
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
using Altinn.Register.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AMPartyService = Altinn.AccessMgmt.Core.Services.AMPartyService;

namespace Altinn.AccessMgmt.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessMgmtCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<RegisterHostedService>();
        services.AddHostedService<AltinnRoleHostedService>();
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
        services.AddScoped<IAmPartyRepository, AMPartyService>();
        services.AddScoped<IAuthorizedPartyRepoService, AuthorizedPartyRepoService>();

        if (configuration.GetValue<bool>("FeatureManagement:AccessMgmt.Core.Services.AuthorizedParties.EfEnabled"))
        {
            services.AddScoped<IAuthorizedPartiesService, AuthorizedPartiesServiceEf>();
        }
        else
        {
            services.AddScoped<IAuthorizedPartiesService, AuthorizedPartiesService>();
        }

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
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesResourceRegistrySync)
                .WithGroupName("Resource Registry Import")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync Service Owners")
                    .WithLease("resource_registry_pipeline_service_owners")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource(ResourceRegistryPipelines.ServiceOwnerTasks.Extract)
                        .AddSegment(ResourceRegistryPipelines.ServiceOwnerTasks.Transform)
                        .AddSink(ResourceRegistryPipelines.ServiceOwnerTasks.Load)
                        .Build()
                .AddPipeline("Sync Resources")
                    .WithLease("resource_registry_pipeline_resources")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource(ResourceRegistryPipelines.ResourceTasks.Extract)
                        .AddSegment(ResourceRegistryPipelines.ResourceTasks.Transform)
                        .AddSink(ResourceRegistryPipelines.ResourceTasks.Load)
                        .Build()
                .AddPipeline("Sync Package Resources")
                    .WithLease("resource_registry_pipeline_package_resources")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource(ResourceRegistryPipelines.UpdatedResourceTasks.Extract)
                        .AddSegment(ResourceRegistryPipelines.PackageResourceTasks.Transform)
                        .AddSink(ResourceRegistryPipelines.PackageResourceTasks.Load)
                        .Build()
                .AddPipeline("Sync Role Resources")
                    .WithLease("resource_registry_pipeline_role_resources")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.ResourceRegistryImportSystem))
                    .WithStages()
                        .AddSource(ResourceRegistryPipelines.UpdatedResourceTasks.Extract)
                        .AddSegment(ResourceRegistryPipelines.RoleResourceTasks.Transform)
                        .AddSink(ResourceRegistryPipelines.RoleResourceTasks.Load)
                        .Build();
        });

        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesRegisterSync)
                .WithGroupName("Register Import (Persons)")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync Persons")
                    .WithLease("register_pipeline_persons")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.RegisterImportSystem))
                    .WithStages()
                        .AddSource(RegisterPipelines.PartyJobs.Extract<Person>)
                        .AddSegment(RegisterPipelines.PartyJobs.Transform)
                        .AddSink(RegisterPipelines.PartyJobs.Load)
                        .Build();
        });

        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesRegisterSync)
                .WithGroupName("Register Import (Organizations)")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync Organizations")
                    .WithLease("register_pipeline_organizations")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.RegisterImportSystem))
                    .WithStages()
                        .AddSource(RegisterPipelines.PartyJobs.Extract<Organization>)
                        .AddSegment(RegisterPipelines.PartyJobs.Transform)
                        .AddSink(RegisterPipelines.PartyJobs.Load)
                        .Build();
        });

        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesRegisterSync)
                .WithGroupName("Register Import (System Users)")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync System users")
                    .WithLease("register_pipeline_systemusers")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.RegisterImportSystem))
                    .WithStages()
                        .AddSource(RegisterPipelines.PartyJobs.Extract<SystemUser>)
                        .AddSegment(RegisterPipelines.PartyJobs.Transform)
                        .AddSink(RegisterPipelines.PartyJobs.Load)
                        .Build();
        });

        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesRegisterSync)
                .WithGroupName("Register Import (Self Identified Users)")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync Self Identified Users")
                    .WithLease("register_pipeline_selfidentified")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.RegisterImportSystem))
                    .WithStages()
                        .AddSource(RegisterPipelines.PartyJobs.Extract<SelfIdentifiedUser>)
                        .AddSegment(RegisterPipelines.PartyJobs.Transform)
                        .AddSink(RegisterPipelines.PartyJobs.Load)
                        .Build();
        });

        services.AddPipelines(descriptor =>
        {
            descriptor
                .WithFeatureFlag(AccessMgmtFeatureFlags.HostedServicesRegisterSync)
                .WithGroupName("Register Import (Self Enterprise Users)")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Sync Enterprise Users")
                    .WithLease("register_pipeline_enterpriseusers")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.RegisterImportSystem))
                    .WithStages()
                        .AddSource(RegisterPipelines.PartyJobs.Extract<EnterpriseUser>)
                        .AddSegment(RegisterPipelines.PartyJobs.Transform)
                        .AddSink(RegisterPipelines.PartyJobs.Load)
                        .Build();
        });
    }

    private static void AddJobs(IServiceCollection services)
    {
        services.AddSingleton<IPartySyncService, PartySyncService>();
        services.AddSingleton<IRoleSyncService, RoleSyncService>();
        services.AddSingleton<IResourceSyncService, ResourceSyncService>();
        services.AddSingleton<IAltinnClientRoleSyncService, AltinnClientRoleSyncService>();
        services.AddSingleton<IAltinnAdminRoleSyncService, AltinnAdminRoleSyncService>();
        services.AddSingleton<IAllAltinnRoleSyncService, AllAltinnRoleSyncService>();
    }
}
