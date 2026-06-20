namespace Altinn.Authorization.Host.Lease.Tests;
[UnitTest]
public class CiDemoLeaseFailures
{
    [Fact]
    public void Lease_ThrowsUnexpectedly() => throw new InvalidOperationException("lease renewal failed unexpectedly");
}
