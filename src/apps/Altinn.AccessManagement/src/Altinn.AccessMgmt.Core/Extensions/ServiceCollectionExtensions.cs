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
        services.AddScoped<IIngestService, IngestService>();
        services.AddScoped<IPartySyncService, PartySyncService>();
        services.AddScoped<IRoleSyncService, RoleSyncService>();
        services.AddScoped<IResourceSyncService, ResourceSyncService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        return services;
    }
}
