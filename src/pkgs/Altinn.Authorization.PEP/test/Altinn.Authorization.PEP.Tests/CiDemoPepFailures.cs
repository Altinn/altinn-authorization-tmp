namespace Altinn.Authorization.PEP.Tests;
[UnitTest]
public class CiDemoPepFailures
{
    [Fact]
    public void Pep_CollectionDiffers_MultiLine() => Assert.Equal(new[] { 1, 2, 3 }, new[] { 1, 9, 3 });
}
