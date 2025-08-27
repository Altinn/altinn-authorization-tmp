namespace Altinn.AccessMgmt.Core.HostedServices;

/// <summary>
/// Lease content
/// </summary>
public class RegisterLease()
{
    /// <summary>
    /// The URL of the next page of Party data.
    /// </summary>
    public string PartyStreamNextPageLink { get; set; }

    /// <summary>
    /// The URL of the next page of AssignmentSuccess data.
    /// </summary>
    public string RoleStreamNextPageLink { get; set; }
}
