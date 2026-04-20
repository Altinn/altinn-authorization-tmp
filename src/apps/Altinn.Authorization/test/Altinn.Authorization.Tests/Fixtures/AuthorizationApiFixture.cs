using Altinn.Common.AccessToken.Services;
using Altinn.Common.Authentication.Configuration;
using Altinn.Platform.Authorization.IntegrationTests.MockServices;
using Altinn.Platform.Authorization.Repositories.Interface;
using Altinn.Platform.Authorization.Services.Interface;
using Altinn.Platform.Authorization.Services.Interfaces;
using Altinn.Platform.Events.Tests.Mocks;
using Altinn.ResourceRegistry.Tests.Mocks;
using AltinnCore.Authentication.JwtCookie;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Altinn.Platform.Authorization.IntegrationTests.Fixtures;

/// <summary>
/// Shared test fixture for Authorization API integration tests.
/// Registers the common mock service set used across all controller test classes,
/// eliminating the need for each test to wire up the full mock graph manually.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="ConfigureServices"/> from the test class constructor to add
/// per-class mock overrides (e.g. <c>IContextHandler</c>).
/// </para>
/// <para>
/// For per-test variation (e.g. injecting a specific <see cref="Microsoft.FeatureManagement.IFeatureManager"/>
/// or <c>IEventsQueueClient</c> mock instance), call
/// <see cref="WebApplicationFactory{TEntryPoint}.WithWebHostBuilder"/> from
/// the test method and register only the varying services — the common mocks
/// from this fixture are inherited automatically.
/// </para>
/// </remarks>
public class AuthorizationApiFixture : WebApplicationFactory<Program>
{
    private readonly List<Action<IServiceCollection>> _configureServicesActions = [];

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Authentication stubs
            services.AddSingleton<IPostConfigureOptions<JwtCookieOptions>, JwtCookiePostConfigureOptionsStub>();
            services.AddSingleton<IPostConfigureOptions<OidcProviderSettings>, OidcProviderPostConfigureSettingsStub>();
            services.AddSingleton<IPublicSigningKeyProvider, PublicSigningKeyProviderMock>();

            // Policy / delegation mocks
            services.AddSingleton<IPolicyRetrievalPoint, PolicyRetrievalPointMock>();
            services.AddSingleton<IPolicyRepository, PolicyRepositoryMock>();
            services.AddSingleton<IDelegationMetadataRepository, DelegationMetadataRepositoryMock>();
            services.AddSingleton<IDelegationChangeEventQueue, DelegationChangeEventQueueMock>();
            services.AddSingleton<IInstanceMetadataRepository, InstanceMetadataRepositoryMock>();

            // External service mocks
            services.AddSingleton<IParties, PartiesMock>();
            services.AddSingleton<IProfile, ProfileMock>();
            services.AddSingleton<IRoles, RolesMock>();
            services.AddSingleton<IOedRoleAssignmentWrapper, OedRoleAssignmentWrapperMock>();
            services.AddSingleton<IRegisterService, RegisterServiceMock>();
            services.AddSingleton<IResourceRegistry, ResourceRegistryMock>();
            services.AddSingleton<IAccessManagementWrapper, AccessManagementWrapperMock>();

            // Per-class overrides
            foreach (var configure in _configureServicesActions)
            {
                configure(services);
            }
        });
    }

    /// <summary>
    /// Registers a callback that modifies the <see cref="IServiceCollection"/>
    /// when building the test host. Call from the test class constructor only.
    /// </summary>
    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _configureServicesActions.Add(configureServices);
    }

    /// <summary>
    /// Creates an <see cref="HttpClient"/> for the test server with optional
    /// configuration callbacks (e.g. setting default headers).
    /// </summary>
    public HttpClient BuildClient(params Action<HttpClient>[] configureClient)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        foreach (var configure in configureClient)
        {
            configure(client);
        }

        return client;
    }
}
