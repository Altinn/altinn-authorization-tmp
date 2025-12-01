using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.ConsoleTester;

public interface IParallelQueryTester
{
    Task RunAsync(int numberOfQueries);
}

public class ParallelQueryTester(IDbContextFactory<ReadOnlyDbContext> factory, ConnectionQuery connectionQuery, ReadOnlyRoundRobinTester roundRobinTester) : IParallelQueryTester
{
    public IDbContextFactory<ReadOnlyDbContext> Factory { get; } = factory;

    public async Task RunAsync(int numberOfQueries)
    {
        var calls = Enumerable.Range(0, numberOfQueries);

        Console.WriteLine($"Starting {numberOfQueries} parallel queries...");

        await Parallel.ForEachAsync(
            calls, 
            new ParallelOptions
            {
                MaxDegreeOfParallelism = numberOfQueries
            },
            async (index, ct) =>
            {
                try
                {
                    var msg = await ExecuteAsync(index);
                    Console.WriteLine($"[{index}] OK: {msg}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{index}] ERROR: {ex.Message}");
                }
            }
        );
    }

    //private async Task<string> ExecuteAsync(int index)
    //{
    //    var res = await connectionQuery.GetConnectionsFromOthersAsync(new ConnectionQueryFilter() { ToIds = [Guid.Parse("1ed8a4e3-6d2b-4cf0-9d8e-25d0439c9c57")] });
    //    return $"Count: {res.Count()}";
    //}

    private async Task<string> ExecuteAsync(int index)
    {
        var rr = new ReadOnlyRoundRobinTester(Factory);
        var res = await rr.Go();
        return $"[{index}]: IP: {res.IP} Db: {res.DB} Slot: {res.Slot}";
    }
}
