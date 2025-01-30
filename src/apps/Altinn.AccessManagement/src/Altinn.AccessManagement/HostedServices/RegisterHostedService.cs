using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Register;

namespace Altinn.Authorization.AccessManagement;

/// <summary>
/// 
/// </summary>
/// <param name="lease"></param>
/// <param name="register"></param>
/// <param name="logger"></param>
public partial class RegisterHostedService(IAltinnLease lease, IAltinnRegister register, ILogger<RegisterHostedService> logger) : IHostedService, IDisposable
{
    private readonly IAltinnLease _lease = lease;

    private readonly IAltinnRegister _register = register;

    private readonly ILogger<RegisterHostedService> _logger = logger;

    private int _executionCount = 0;

    private Timer _timer = null;

    private readonly CancellationTokenSource _stop = new();

    private static readonly IEnumerable<string> _registerFields = [
        "party",
        "organization",
        "person",
        "identifiers",
        "party-uuid",
        "party-version-id",
        "party-is-deleted",
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

        _timer = new Timer(SyncRegisterRoundTripper, _stop.Token, TimeSpan.Zero, TimeSpan.FromMinutes(2));

        return Task.CompletedTask;
    }

    private void SyncRegisterRoundTripper(object state)
    {
        var cancellationToken = (CancellationToken)state;
        Interlocked.Increment(ref _executionCount);
        SyncRegister(cancellationToken).Wait();
    }

    private async Task SyncRegister(CancellationToken cancellationToken)
    {
        await using var lease = await _lease.TryAquireNonBlocking<LeaseContent>("access_management_register_sync", cancellationToken);
        if (!lease.HasLease || cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            await foreach (var page in await _register.Stream(lease?.Data?.NextPageLink, _registerFields, cancellationToken))
            {
                foreach (var item in page.Items)
                {
                    Log.Party(_logger, item.PartyUuid);
                    // TODO: Write Page to Service Bus here || db!
                }

                await _lease.Put(lease, new(page.Links.Next), cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Log.SyncError(_logger, ex);
            return;
        }
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
    /// Disposing
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer?.Dispose();
        }
    }

    /// <summary>
    /// Lease Content
    /// </summary>
    /// <param name="nextPageLink">next page url</param>
    public class LeaseContent(string nextPageLink)
    {
        /// <summary>
        /// Url of next page
        /// </summary>
        public string NextPageLink { get; set; } = nextPageLink;
    }

    private static partial class Log
    {
        [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "Received party {partyUuid} from register")]
        internal static partial void Party(ILogger logger, string partyUuid);

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "An error occured while streaming data from register")]
        internal static partial void SyncError(ILogger logger, Exception ex);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Starting register hosted service")]
        internal static partial void StartRegisterSync(ILogger logger);

        [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Quit register hosted service")]
        internal static partial void QuitRegisterSync(ILogger logger);
    }
}
