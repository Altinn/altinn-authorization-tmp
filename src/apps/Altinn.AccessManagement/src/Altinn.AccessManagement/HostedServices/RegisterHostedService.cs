using Altinn.AccessMgmt.AccessPackages.Repo.Data.Contracts;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Register;
using Altinn.Authorization.Integration.Register.Models;

namespace Altinn.Authorization.AccessManagement;

/// <summary>
/// A hosted service responsible for synchronizing register data using leases.
/// </summary>
/// <param name="lease">Lease provider for distributed locking.</param>
/// <param name="register">Register integration service.</param>
/// <param name="entityService"></param>
/// <param name="entityTypeService"></param>
/// <param name="entityVariantService"></param>
/// <param name="entityLookupService"></param>
/// <param name="logger">Logger for logging service activities.</param>
public partial class RegisterHostedService(
    IAltinnLease lease, 
    IAltinnRegister register, 
    IEntityService entityService,
    IEntityTypeService entityTypeService,
    IEntityVariantService entityVariantService,
    IEntityLookupService entityLookupService,
    ILogger<RegisterHostedService> logger) : IHostedService, IDisposable
{
    private readonly IAltinnLease _lease = lease;
    private readonly IAltinnRegister _register = register;
    private readonly IEntityService entityService = entityService;
    private readonly IEntityTypeService entityTypeService = entityTypeService;
    private readonly IEntityVariantService entityVariantService = entityVariantService;
    private readonly IEntityLookupService entityLookupService = entityLookupService;
    private readonly ILogger<RegisterHostedService> _logger = logger;
    private int _executionCount = 0;
    private Timer _timer = null;
    private readonly CancellationTokenSource _stop = new();

    /// <summary>
    /// List of register fields to be retrieved during synchronization.
    /// </summary>
    private static readonly IEnumerable<string> _registerFields = [
        "party",
        "organization",
        "person",
        "identifiers",
        "party-uuid",
        "party-version-id",
        "organization-business-address",
        "organization-mailing-address",
        "organization-internet-address",
        "organization-email-address",
        "organization-fax-number",
        "organization-mobile-number",
        "organization-telephone-number",
        "organization-unit-type",
        "organization-unit-status",
        "person-date-of-death",
        "person-mailing-address",
        "person-address",
        "person-last-name",
        "person-middle-name",
        "person-first-name",
        "party-modified-at",
        "party-created-at",
        "party-organization-identifier",
        "party-person-identifier",
        "party-name",
        "party-type",
        "party-id",
        "person-date-of-birth",
        "sub-units"
    ];

    /// <inheritdoc/>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Log.StartRegisterSync(_logger);

        _timer = new Timer(SyncRegisterDispatcher, _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

        return Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches the register synchronization process in a separate task.
    /// </summary>
    /// <param name="state">Cancellation token for stopping execution.</param>
    private void SyncRegisterDispatcher(object state)
    {
        var cancellationToken = (CancellationToken)state;
        SyncRegister(cancellationToken).Wait();
    }

    /// <summary>
    /// Synchronizes register data by first acquiring a remote lease and streaming register entries.
    /// Returns if lease is already taken.
    /// </summary>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    private async Task SyncRegister(CancellationToken cancellationToken)
    {
        await using var ls = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_register_sync", cancellationToken);
        if (!ls.HasLease || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var types = await entityTypeService.Get();
        var variants = await entityVariantService.Get();

        try
        {
            await foreach (var page in await _register.Stream(ls.Data?.NextPageLink, _registerFields, cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                foreach (var item in page.Items)
                {
                    Interlocked.Increment(ref _executionCount);
                    Log.Party(_logger, item.PartyUuid, _executionCount);

                    await entityService.ExtendedRepo.Upsert(Guid.Parse(item.PartyUuid), new AccessMgmt.Models.Entity()
                    {
                        Id = Guid.Parse(item.PartyUuid),
                        Name = item.Name,
                        RefId = item.PersonIdentifier ?? item.OrganizationIdentifier,
                        TypeId = types.First(t => t.Name.Equals("Organisasjon")).Id,
                        VariantId = variants.First(t => t.Name.Equals("AS")).Id
                    });

                    await WriteToDb(item);
                }

                if (!string.IsNullOrEmpty(page.Links.Next))
                {
                    await _lease.Put(ls, new() { NextPageLink = page.Links.Next }, cancellationToken);
                }
                else
                {
                    return;
                }

                await _lease.RefreshLease(ls, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.SyncError(_logger, ex);
            return;
        }
    }

    /// <summary>
    /// Writes the synchronized register data to the database.
    /// </summary>
    /// <param name="model">Party model containing register data.</param>
    /// <returns>A completed task.</returns>
    public Task WriteToDb(PartyModel model)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            Log.QuitRegisterSync(_logger);
        }
        finally
        {
            _timer?.Change(Timeout.Infinite, 0);
            _stop.Cancel();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged resources.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is called from Dispose.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _stop?.Dispose();
            _timer?.Dispose();
        }
    }

    /// <summary>
    /// Represents lease content, including pagination link.
    /// </summary>
    public class LeaseContent()
    {
        /// <summary>
        /// The URL of the next page of data.
        /// </summary>
        public string NextPageLink { get; set; }
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Processing party with uuid {partyUuid} from register. Count {count}")]
        internal static partial void Party(ILogger logger, string partyUuid, int count);

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An error occured while streaming data from register")]
        internal static partial void SyncError(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Starting register hosted service")]
        internal static partial void StartRegisterSync(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Quit register hosted service")]
        internal static partial void QuitRegisterSync(ILogger logger);
    }
}
