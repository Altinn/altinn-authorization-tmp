namespace Altinn.Authorization.Host.Lease;

/// <summary>
/// 
/// </summary>
public class AltinnLeaseOptions
{
    public AltinnLeaseType Type { get; set; }

    public StorageAccountLease StorageAccount { get; set; }

    public class StorageAccountLease
    {
        public Uri Endpoint { get; set; }

        internal static string Name { get; set; } = "Leases";

        internal static string Container { get; set; } = "leases";
    }
}

/// <summary>
/// Mass transit transports.
/// </summary>
public enum AltinnLeaseType
{
    /// <summary>
    /// In memory transport. Only used for testing.
    /// </summary>
    InMemory = default,

    /// <summary>
    /// Azure Storage Account.
    /// </summary>
    AzureStorageAccount,
}
