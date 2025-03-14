using Altinn.Authorization.Integration.Platform;
using Altinn.Authorization.Integration.Platform.ResourceRegister;

namespace Altinn.Authorization.Integration.Tests.ResourceRegister;

/// <summary>
/// Contains test cases for the ResourceGetEndpoint.
/// </summary>
public class ResourceGetEndpointTest : IClassFixture<PlatformFixture>
{
    private PlatformFixture Fixture { get; }

    private IAltinnResourceRegister ResourceRegister { get; }

    public ResourceGetEndpointTest(PlatformFixture fixture)
    {
        Fixture = fixture;
        fixture.SkipIfMissingConfiguration<AltinnResourceRegisterOptions>();
        fixture.SkipIfMissingConfiguration<AltinnIntegrationOptions>();
        ResourceRegister = fixture.GetService<IAltinnResourceRegister>();
    }

    /// <summary>
    /// Gets a specific resource from resource registry
    /// </summary>
    /// <param name="resourceId">resource ID</param>
    /// <returns></returns>
    [Theory]
    [InlineData("altinn_access_management")]
    public async Task GetResource(string resourceId)
    {
        var result = await ResourceRegister.GetResource(resourceId, TestContext.Current.CancellationToken);
        Assert.True(result.IsSuccessful);
    }
}
