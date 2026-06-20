namespace Altinn.Authorization.Host.Lease.Tests;

// DEMO: intentionally failing test. Remove before merging to main.
[UnitTest]
public class CiDemoLeaseFailures
{
    [Fact]
    public void Lease_ThrowsUnexpectedly() => throw new InvalidOperationException("lease renewal failed unexpectedly");
}
