namespace Altinn.Authorization.AccessManagement.HostedServices;

/// <summary>
/// Lease content
/// </summary>
public class AllAltinnRoleLease()
{
    /// <summary>
    /// The URL of the next page of All Altinn roles data.
    /// </summary>
    public string AllAltinnRoleStreamNextPageLink { get; set; }

    /// <summary>
    /// The URL of the next page of Altinn client roles data.
    /// </summary>
    public string AltinnClientRoleStreamNextPageLink { get; set; }

    /// <summary>
    /// The URL of the next page of Altinn admin roles data.
    /// </summary>
    public string AltinnAdminRoleStreamNextPageLink { get; set; }

    /// <summary>
    /// The URL of the next page of Altinn bankruptcy estate roles data.
    /// </summary>
    public string AltinnBankruptcyEstateRoleStreamNextPageLink { get; set; }
}
