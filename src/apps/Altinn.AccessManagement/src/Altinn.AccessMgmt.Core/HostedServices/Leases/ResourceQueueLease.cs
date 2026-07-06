namespace Altinn.AccessMgmt.Core.HostedServices.Leases;

/// <summary>
/// Lease content
/// </summary>
internal class ResourceQueueLease()
{
    /// <summary>
    ///  The id of the next queue item to fetch (inclusive).
    /// </summary>
    public long NextElementToFetch { get; set; } = 1;
}
