namespace Altinn.Authorization.Host.Pipeline.Tests;

// DEMO: intentionally failing tests that exercise the CI failed-test reporting on
// this PR. Remove this file (and the other CiDemo*.cs) before merging to main.
[UnitTest]
public class CiDemoPipelineFailures
{
    [Fact]
    public void Pipeline_StringsDiffer_MultiLineMessage() => Assert.Equal("pipeline: ready", "pipeline: reedy");

    [Theory]
    [InlineData(3)]
    public void Pipeline_TheoryFails(int n) => Assert.True(n > 100, $"expected {n} to exceed 100");
}
