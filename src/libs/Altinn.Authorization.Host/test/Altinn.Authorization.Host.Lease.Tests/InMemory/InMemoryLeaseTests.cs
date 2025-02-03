using Altinn.Authorization.Host.Lease.InMemory;

namespace Altinn.Authorization.Host.Lease.Tests;

/// <summary>
/// 
/// </summary>
public class InMemoryLeaseTests : FanoutTests
{
    public override IAltinnLease Lease { get; set; } = new InMemoryLease();

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public async Task TestLease(int numThreads)
    {
        await TestThreadAquireExplosion(numThreads);
    }
}
