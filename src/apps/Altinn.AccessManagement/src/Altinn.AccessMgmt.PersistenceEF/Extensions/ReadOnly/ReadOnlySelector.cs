using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using static Altinn.AccessMgmt.PersistenceEF.Extensions.ServiceCollectionExtensions;

namespace Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;

public class ReadOnlySelector : IReadOnlySelector
{
    private readonly IServiceProvider _sp;
    private readonly AccessManagementDatabaseOptions _options;
    private readonly Dictionary<string, string> _namedReplicas;
    private readonly string[] _roundRobinPool;

    private static int _globalIndex = -1;

    public ReadOnlySelector(IServiceProvider sp)
    {
        _sp = sp;
        _options = _sp.GetRequiredService<IOptions<AccessManagementDatabaseOptions>>().Value;

        _namedReplicas = new Dictionary<string, string>(
            _options.ReadOnlyConnectionStrings ?? new(),
            StringComparer.OrdinalIgnoreCase
            );

        _namedReplicas["Primary"] = _options.AppConnectionString;

        var readonlySources = _options.ReadOnlyConnectionStrings?.Values ?? Enumerable.Empty<string>();

        _roundRobinPool = _options.IncludePrimaryInReadOnlyPool
            ? readonlySources.Append(_options.AppConnectionString).ToArray()
            : readonlySources.ToArray();
    }

    public string GetConnectionString()
    {
        if (_roundRobinPool.Length == 0)
        {
            return _options.AppConnectionString;
        }

        if (_options.EnableReadOnlyHints)
        {
            var hintService = _sp.GetRequiredService<IReadOnlyHintService>();
            var hint = hintService.GetHint();

            if (hint != null && _namedReplicas.TryGetValue(hint, out var hintedConn))
            {
                return hintedConn;
            }
        }

        var idx = Interlocked.Increment(ref _globalIndex);
        return _roundRobinPool[idx % _roundRobinPool.Length];
    }
}
