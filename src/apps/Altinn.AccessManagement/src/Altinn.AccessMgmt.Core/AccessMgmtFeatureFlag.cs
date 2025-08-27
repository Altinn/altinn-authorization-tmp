namespace Altinn.AccessMgmt.Core;

/// <summary>
/// Access Management Feature Flags.
/// </summary>
internal static class AccessMgmtFeatureFlags
{
    /// <summary>
    /// Specifies if the register data should streamed from register service to access management database
    /// </summary>
    public const string HostedServicesRegisterSync = $"AccessManagement.HostedServices.RegisterSync";

    /// <summary>
    /// Specifies if the register data should streamed from register service to access management database
    /// </summary>
    public const string HostedServicesResourceRegistrySync = $"AccessManagement.HostedServices.ResourceRegistrySync";

    /// <summary>
    /// Specifies if migration should be done.
    /// </summary>
    public const string MigrationDb = $"AccessManagement.MigrationDb";

    /// <summary>
    /// Specifies if entity framework should manage migration.
    /// </summary>
    public const string MigrationDbEf = $"AccessManagement.MigrationDbEf";

    /// <summary>
    /// Specifies if migration should be done and include basic data for testing.
    /// </summary>
    public const string MigrationDbWithBasicData = $"AccessManagement.MigrationDbWithBasicData";
}
