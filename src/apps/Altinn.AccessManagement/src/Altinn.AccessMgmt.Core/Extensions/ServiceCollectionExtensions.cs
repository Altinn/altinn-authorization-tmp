using Altinn.AccessMgmt.Core.HostedServices;
using Altinn.Authorization.Host.Job;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.Core.Extensions;

/// <summary>
/// Extension methods for adding access management core services to the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Extension methods for adding access management core services to the dependency injection container.
    /// </summary>
    /// <param name="builder">web application builder</param>
    public static void AddAccessMgmtCore(this IServiceCollection services)
    {
        services.AddJobs("AccessMgmt.Register", jobs =>
        {
            jobs.Add<RegisterPartyJob>();
            jobs.Add<RegisterRoleJob>(opts =>
            {
                opts.DependsOn<RegisterPartyJob>();
            });
        });

        services.AddJobs("AccessMgmt.ResourceRegistry", jobs =>
        {
            // jobs.Add<ResourceSyncJob>(opts =>
            // {
            //     opts.DependsOn<RegisterRoleJob>();
            // });
        });
    }
}
