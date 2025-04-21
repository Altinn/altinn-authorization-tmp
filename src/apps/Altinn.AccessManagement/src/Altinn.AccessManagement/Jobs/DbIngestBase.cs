using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Host.Lease;

namespace Altinn.AccessManagement.HostedServices.Jobs;

/// <summary>
/// Base
/// </summary>
public abstract class DbIngestBase
{
    /// <summary>
    /// Sepcifies which system made the change
    /// </summary>
    protected ChangeRequestScope StartChangeRequest()
    {
        if (CurrentChangeRequest == null)
        {
            ChangeRequest = new ChangeRequestScope(this);
        }

        return ChangeRequest;
    }

    protected ChangeRequestOptions? CurrentChangeRequest => ChangeRequest?.Instance;

    protected ChangeRequestScope? ChangeRequest { get; set; } = null;

    /// <summary>
    /// Upsert Lease
    /// </summary>
    /// <typeparam name="T"></typeparam>
    protected async Task UpsertAndRefreshLease<T>(IAltinnLease lease, LeaseResult<T> ls, Action<T> configureLeaseContent, CancellationToken cancellationToken)
        where T : class, new()
    {
        if (ls.Data == null)
        {
            await lease.Put(ls, new T(), cancellationToken);
        }
        else
        {
            configureLeaseContent(ls.Data);
        }

        await lease.RefreshLease(ls, cancellationToken);
    }

    public virtual Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public class ChangeRequestScope : IDisposable
    {
        private DbIngestBase Base { get; }

        public readonly ChangeRequestOptions Instance = new ChangeRequestOptions
        {
            ChangedBy = AuditDefaults.RegisterImportSystem,
            ChangedBySystem = AuditDefaults.RegisterImportSystem,
        };

        public ChangeRequestScope(DbIngestBase instance)
        {
            Base = instance;
        }

        public static implicit operator ChangeRequestOptions(ChangeRequestScope scope)
        {
            return scope.Instance;
        }

        public void Dispose()
        {
            Base.ChangeRequest = null;
        }
    }
}
