namespace Altinn.Authorization.Integration.Platform.AltinnRole;

/// <summary>
/// Configuration options for the Altinn Role integration.
/// </summary>
public class AltinnRoleOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRoleOptions"/> class.
    /// </summary>
    public AltinnRoleOptions()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnRoleOptions"/> class 
    /// and applies the provided configuration action to set up the options.
    /// </summary>
    /// <param name="configureOptions">
    /// A delegate that configures the <see cref="AltinnRoleOptions"/> instance.
    /// </param>
    public AltinnRoleOptions(Action<AltinnRoleOptions> configureOptions)
    {
        configureOptions(this);
    }

    /// <summary>
    /// Gets or sets the endpoint URL for the Altinn Role service.
    /// </summary>
    /// <remarks>
    /// This URL is used for making requests to the Altinn Role API.
    /// </remarks>
    public Uri Endpoint { get; set; }
}
