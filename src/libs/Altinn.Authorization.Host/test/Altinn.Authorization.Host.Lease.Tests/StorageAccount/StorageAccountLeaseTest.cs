using Altinn.Authorization.Host.Lease.StorageAccount;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.Host.Lease.Tests;

/// <summary>
/// 
/// </summary>
public class StorageAccountLeaseTest : FanoutTests
{
    public override IAltinnLease Lease { get; set; }

    public StorageAccountLeaseTest()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            // builder.AddConsole();
        });
        AltinnLease.ConfigureAltinnLeaseAzureStorageAccount(services, new()
        {
            Type = AltinnLeaseType.AzureStorageAccount,
            StorageAccount = new()
            {
                Endpoint = new Uri("https://{storage_account_name}.core.windows.net/"),
            }
        });

        Lease = services.BuildServiceProvider().GetRequiredService<IAltinnLease>() as StorageAccountLease;
    }

    [Theory(Skip = "Requires Strorage Account")]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]

    public async Task TestLease(int numThreads)
    {
        await TestThreadAquireExplosion(numThreads);
    }
}
