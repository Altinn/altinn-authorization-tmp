namespace Altinn.AccessMgmt.Core;

public static class AccessMgmtFeatureFlags
{
    /// <summary>
    /// Ensures that all pipelines has at executed before application starts.
    /// </summary>
    public const string PipelineInit = "AccessManagement.Core.Pipeline.Init";

    /// <summary>
    /// Specifies if the register data should streamed from register service to access management database
    /// </summary>
    public const string HostedServicesRegisterSync = $"AccessMgmt.Core.HostedServices.RegisterSync";

    /// <summary>
    /// Specifies if the register data should streamed from register service to access management database
    /// </summary>
    public const string HostedServicesRegisterSyncImport = $"AccessMgmt.Core.HostedServices.RegisterSync.Import";

    /// <summary>
    /// Specifies if the register data should streamed from register service to access management database
    /// </summary>
    public const string HostedServicesResourceRegistrySync = $"AccessMgmt.Core.HostedServices.ResourceRegistrySync";

    /// <summary>
    /// Specifies if the altinn roles data should streamed from sblbridge service to access management database
    /// </summary>
    public const string HostedServicesAllAltinnRoleSync = $"AccessMgmt.Core.HostedServices.AllAltinnRoleSync";

    /// <summary>
    /// Specifies if the altinn client roles data should streamed from sblbridge service to access management database
    /// </summary>
    public const string HostedServicesAltinnClientRoleSync = $"AccessMgmt.Core.HostedServices.AltinnClientRoleSync";

    /// <summary>
    /// Specifies if the altinn admin roles data should streamed from sblbridge service to access management database
    /// </summary>
    public const string HostedServicesAltinnAdminRoleSync = $"AccessMgmt.Core.HostedServices.AltinnAdminRoleSync";

    /// <summary>
    /// Specifies if AuthorizedPartiesServiceEf should be used
    /// </summary>
    public const string AuthorizedPartiesEfEnabled = $"AccessMgmt.Core.Services.AuthorizedParties.EfEnabled";
}
