using Altinn;
using Altinn.AccessMgmt;
using Altinn.AccessMgmt.AccessPackages;
using Altinn.AccessMgmt.Repo;
using Altinn.AccessMgmt.Repo.Migrate;
using Altinn.AccessMgmt.DbAccess.Data.Models;
using Altinn.AccessMgmt.DbAccess.Migrate.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo;
using Altinn.AccessMgmt.Repo.Migrate;
using Altinn.Authorization.Host.Lease;
using System.Threading;

namespace Altinn.AccessMgmt.Repo.Migrate;

/// <summary>
/// Access Package Migration
/// </summary>
public class DatabaseMigration
{
    private readonly IAltinnLease lease;
    private readonly IDbMigrationFactory factory;

    private bool Enable { get { return factory.Enable; } }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="factory">IDbMigrationFactory</param>
    public DatabaseMigration(IAltinnLease lease, IDbMigrationFactory factory)
    {
        this.lease=lease;
        this.factory = factory;
    }
    public class LeaseContent()
    {
        /// <summary>
        /// Last update date
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }
    }

    /// <inheritdoc/>
    public async Task Init(CancellationToken cancellationToken = default)
    {
        if (Enable)
        {
            await using var ls = await lease.TryAquireNonBlocking<LeaseContent>("access_management_db_migrate", cancellationToken);
            if (!ls.HasLease || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            await factory.Init();

            await lease.Put(ls, new() { UpdatedAt = DateTimeOffset.Now }, cancellationToken);
            // await lease.RefreshLease(ls, cancellationToken);
        }
    }
}
