namespace Altinn.Authorization.PEP.Tests;

// DEMO: intentionally failing test. Remove before merging to main.
[UnitTest]
public class CiDemoPepFailures
{
    [Fact]
    public void Pep_CollectionDiffers_MultiLine() => Assert.Equal(new[] { 1, 2, 3 }, new[] { 1, 9, 3 });
}
