namespace Altinn.Authorization.Host.Lease.Appsettings;

/// <summary>
/// Represents the application settings for the Altinn authorization.
/// This class encapsulates configuration settings required for different default applications,
/// such as settings for Azure App Configuration, Entra ID, Service Bus, PostgreSQL,
/// Application Insights, and other relevant configurations.
/// </summary>
public class LeaseSettings
{
    /// <summary>
    /// Appsettings
    /// </summary>
    public Uri StorageAccount { get; set; }
}
