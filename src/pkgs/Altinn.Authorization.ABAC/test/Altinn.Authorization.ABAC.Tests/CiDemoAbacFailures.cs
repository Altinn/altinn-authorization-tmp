namespace Altinn.Authorization.ABAC.Tests;

// DEMO: intentionally failing test. Remove before merging to main.
[UnitTest]
public class CiDemoAbacFailures
{
    [Fact]
    public void Abac_DecisionMismatch() => "NotApplicable".Should().Be("Deny", "the first-applicable rule should deny");
}
