namespace Altinn.AccessMgmt.Core.HostedServices.Leases;

/// <summary>
/// Lease content
/// </summary>
internal class ResourceQueueLease()
{
    /// <summary>
    /// The URL of the next page of All Altinn roles data.
    /// </summary>
    public long NextElementToFetch { get; set; }
}
