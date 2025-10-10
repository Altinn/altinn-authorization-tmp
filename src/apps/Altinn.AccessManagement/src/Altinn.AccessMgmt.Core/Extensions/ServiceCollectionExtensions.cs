using System.Runtime.CompilerServices;
using Altinn.AccessManagement.Core;
using Altinn.AccessManagement.Core.Repositories.Interfaces;
using Altinn.AccessMgmt.Core.HostedServices;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Utils;
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

        Test(services);

        AddJobs(services);
        return services;
    }

    private static void AddJobs(IServiceCollection services)
    {
        services.AddSingleton<IPartySyncService, PartySyncService>();
        services.AddSingleton<IRoleSyncService, RoleSyncService>();
        services.AddSingleton<IResourceSyncService, ResourceSyncService>();
    }

    public static void Test(IServiceCollection services)
    {
        services
            .AddPipelinesOtel()
            .AddPipelines(builder => builder
                .WithGroupName("Register")
                .WithRecurring(TimeSpan.FromMinutes(2))
                .AddPipeline("Party Import")
                    .WithLease("sync_register")
                    .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.InternalApi))
                    .WithStages()
                        .AddSource(NewSource)
                        .AddSegment(NewPipelineSegment())
                        .AddSink(NewPipelineSink)
                        .Build()
                .AddPipeline("External Role Import")
                .WithLease("sync_external_role")
                .WithServiceScope(sp => sp.CreateEFScope(SystemEntityConstants.InternalApi))
                .WithStages()
                    .AddSource(NewSource)
                    .AddSegment(NewPipelineSegment())
                    .AddSink(NewPipelineSink)
                    .Build()
            );
    }

    public static Task NewPipelineSink(PipelineSinkContext<int> kake)
    {
        if (kake.Data == 2)
        {
            Console.WriteLine(kake);
        }

        Console.WriteLine("ASDKASD");
        return Task.CompletedTask;
    }

    public static PipelineSegment<string, int> NewPipelineSegment()
    {
        return static async (ctx) =>
        {
            if (ctx.Data == "kake")
            {
                return 2;
            }

            return 1;
        };
    }

    public static async IAsyncEnumerable<string> NewSource(PipelineSourceContext context, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var d in Enumerable.Range(0, 100))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            yield return "kake";
        }

        yield break;
    }
}
