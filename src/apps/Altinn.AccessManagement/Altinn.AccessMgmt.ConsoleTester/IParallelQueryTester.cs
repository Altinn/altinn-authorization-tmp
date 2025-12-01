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

                    //var rr = scope.ServiceProvider.GetRequiredService<ReadOnlyRoundRobinTester>();
                    //var res = await rr.Go(ct);
                    //Console.WriteLine($"[{index}] Server: {res.DB}, IP: {res.IP}, Slot: {res.Slot}");

                    var rr = scope.ServiceProvider.GetRequiredService<ReadOnlyRoundRobinTester2>();
                    var res = await rr.Go(ct);
                    Console.WriteLine($"[{index}] {res}");

                    //var rr = scope.ServiceProvider.GetRequiredService<ConnectionQuery>();
                    //var res = await rr.GetConnectionsFromOthersAsync(new ConnectionQueryFilter() { ToIds = [Guid.Parse("1ed8a4e3-6d2b-4cf0-9d8e-25d0439c9c57")] }, true, ct);
                    //Console.WriteLine($"[{index}] Count: {res.Count}");

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
