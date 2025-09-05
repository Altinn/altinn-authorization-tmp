using System.CodeDom.Compiler;
using System.Diagnostics.Metrics;
using Altinn.Authorization.Host.Lease.StorageAccount;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.Host.Lease.Tests
{
    /// <summary>
    /// This class contains tests for acquiring leases on an Azure Storage Account using Altinn Lease.
    /// The tests validate the proper functionality of lease acquisition and management in a multithreaded environment.
    /// </summary>
    public class StorageAccountLeaseTest : FanoutTests
    {
        /// <summary>
        /// Gets or sets the lease instance used in the tests.
        /// </summary>
        public override ILeaseService Lease { get; set; }

        /// <summary>
        /// Initializes the test setup by configuring the necessary services and Azure Storage Account lease.
        /// </summary>
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
                    BlobEndpoint = new Uri("https://{storage_account}.blob.core.windows.net/"),
                }
            });

            Lease = services.BuildServiceProvider().GetRequiredService<ILeaseService>() as StorageAccountLeaseService;
        }

        /// <summary>
        /// Tests the lease acquisition with multiple threads, simulating concurrent access.
        /// This test verifies that the lease can be successfully acquired by different threads.
        /// </summary>
        /// <param name="numThreads">The number of threads to simulate for lease acquisition.</param>
        /// <returns>A task that represents the asynchronous test operation.</returns>
        [Theory(Skip = "Need a valid storage account")]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public async Task TestLease(int numThreads)
        {
            await TestThreadAquireExplosion(numThreads);
        }

        [Fact(Skip = "Need a valid storage account")]
        public async Task TestLeaseAutoRefresh()
        {
            await using var lease = await Lease.TryAcquireNonBlocking("andreas_test");

            for (var i = 0; i < 100; i++)
            {
                await lease.Update(new LeaseData()
                {
                    Counter = i,
                });
            }

            await Task.Delay(TimeSpan.FromSeconds(90), CancellationToken.None);
            var result = await lease.Get<LeaseData>(default);
            Assert.Equal(99, result.Counter);
        }
    }

    public class LeaseData
    {
        public int Counter { get; set; }
    }
}
