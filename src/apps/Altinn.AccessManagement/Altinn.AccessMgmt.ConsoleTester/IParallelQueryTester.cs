using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.AccessMgmt.ConsoleTester;

public interface IParallelQueryTester
{
    Task RunAsync(int numberOfQueries, CancellationToken cancellationToken);
}

public class ParallelQueryTester : IParallelQueryTester
{
    private readonly IServiceProvider _services;

    public ParallelQueryTester(IServiceProvider services)
    {
        _services = services;
    }

    public async Task RunAsync(int numberOfQueries, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Starting {numberOfQueries} parallel queries...");

        var calls = Enumerable.Range(0, numberOfQueries);

        await Parallel.ForEachAsync(
            calls,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = numberOfQueries,
                CancellationToken = cancellationToken
            },
            async (index, ct) =>
            {
                try
                {
                    // Create new scope for each parallel worker
                    using var scope = _services.CreateScope();

                    var rr = scope.ServiceProvider.GetRequiredService<ReadOnlyRoundRobinTester>();

                    // Execute query
                    var res = await rr.Go(ct);

                    Console.WriteLine($"[{index}] Server: {res.DB}, IP: {res.IP}, Slot: {res.Slot}");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine($"[{index}] CANCELLED");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{index}] ERROR: {ex.Message}");
                }
            }
        );
    }
}
