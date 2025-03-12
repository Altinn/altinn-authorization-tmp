using Microsoft.AspNetCore.Http;

namespace Altinn.Authorization.Integration.Platform.Appsettings;

/// <summary>
/// Represents the platform-related configuration settings for Altinn authorization.
/// This class provides endpoints for interacting with key platform services.
/// </summary>
public class PlatformSettings
{
    /// <summary>
    /// Gets or sets the endpoint URI for the Resource Register service.
    /// This service provides metadata about resources in the Altinn ecosystem.
    /// </summary>
    /// <remarks>
    /// The endpoint should be a valid URI, typically pointing to an API service.
    /// </remarks>
    public EndpointOptions ResourceRegister { get; set; } = new();

    /// <summary>
    /// Gets or sets the endpoint URI for the Register service.
    /// This service is used for managing organizational and individual registry data within Altinn.
    /// </summary>
    /// <remarks>
    /// The endpoint should be a valid URI, typically pointing to an API service.
    /// </remarks>
    public EndpointOptions Register { get; set; } = new();

    public TokenOptions Token { get; set; } = new();

    public class TokenOptions
    {
        public EndpointOptions KeyVault { get; set; }

        public TestToolOptions TestTool { get; set; }

        public class TestToolOptions : EndpointOptions
        {
            public string Username { get; set; }

            public string Password { get; set; }

            public string Environment { get; set; }
        }
    }

    public class EndpointOptions
    {
        public Uri Endpoint { get; set; }
    }
}
