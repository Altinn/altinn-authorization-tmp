namespace Altinn.Authorization.ABAC.Tests;
[UnitTest]
public class CiDemoAbacFailures
{
    [Fact]
    public void Abac_DecisionMismatch() => "NotApplicable".Should().Be("Deny", "the first-applicable rule should deny");
}
