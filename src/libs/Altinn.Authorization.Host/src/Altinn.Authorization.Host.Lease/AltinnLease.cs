using Altinn.Authorization.Host.Lease.Memory;
using Altinn.Authorization.Host.Lease.StorageAccount;
using Altinn.Authorization.ServiceDefaults;
using Azure.Core;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;

namespace Altinn.Authorization.Host.Lease;

public static class AltinnLease
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="builder"></param>
    /// <param name=""></param>
    public static IHostApplicationBuilder AddAltinnLease(this IHostApplicationBuilder builder, Action<AltinnLeaseOptions> configureOptions = null)
    {
        var options = new AltinnLeaseOptions();
        configureOptions?.Invoke(options);
        ConfigureAltinnLease(builder.Services, options)();
        return builder;
    }

    private static Action ConfigureAltinnLease(IServiceCollection services, AltinnLeaseOptions options) => options.Type switch
    {
        AltinnLeaseType.InMemory => () => ConfigureAltinnLeaseInMemory(services),
        AltinnLeaseType.AzureStorageAccount => () => ConfigureAltinnLeaseAzureStorageAccount(services, options),
        _ => throw new InvalidOperationException(""),
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public static void ConfigureAltinnLeaseInMemory(IServiceCollection services)
    {
        services.AddSingleton<IAltinnLease, OptimisticLease>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services">services</param>
    /// <param name="options">options</param>
    /// <param name="descriptor">descriptor</param>
    public static void ConfigureAltinnLeaseAzureStorageAccount(IServiceCollection services, AltinnLeaseOptions options)
    {
        services.AddAzureClients(builder =>
        {
            builder.UseCredential(DefaultTokenCredential.Instance);
            builder.AddBlobServiceClient(options.StorageAccount.Endpoint)
                .WithName(AltinnLeaseOptions.StorageAccountLease.Name)
                .ConfigureOptions(options =>
                {
                    options.Retry.Mode = RetryMode.Exponential;
                    options.Retry.MaxDelay = TimeSpan.FromSeconds(3);
                    options.Retry.MaxRetries = 5;
                });
        });

        services.AddSingleton<IAltinnLease, StorageAccountLease>();
    }
}
