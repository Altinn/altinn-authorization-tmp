using Altinn.AccessMgmt.Core.HostedServices;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Services;
using Altinn.AccessMgmt.Core.Services;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.AccessMgmt.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccessMgmtCore(this IServiceCollection services)
    {
        services.AddHostedService<RegisterHostedService>();
        services.TryAddScoped<IIngestService, IngestService>();
        services.AddSingleton<IPartySyncService, PartySyncService>();
        services.AddSingleton<IRoleSyncService, RoleSyncService>();
        services.AddSingleton<IResourceSyncService, ResourceSyncService>();
        services.TryAddScoped<IAssignmentService, AssignmentService>();

        services.TryAddScoped<IConnectionService, ConnectionService>();
        services.TryAddScoped<IDelegationService, DelegationService>();
        services.TryAddScoped<IRoleService, RoleService>();
        services.TryAddScoped<IPackageService, PackageService>();
        services.TryAddScoped<IResourceService, ResourceService>();
        services.TryAddScoped<IEntityService, EntityService>();

        return services;
    }
}
