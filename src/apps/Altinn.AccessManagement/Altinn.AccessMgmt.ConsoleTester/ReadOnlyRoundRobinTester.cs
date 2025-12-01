using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions.ReadOnly;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.ConsoleTester;

public class ReadOnlyRoundRobinTester
{
    private readonly IDbContextFactory<ReadOnlyDbContext> _factory;
    private readonly IReadOnlySelector _selector;

    public ReadOnlyRoundRobinTester(
        IDbContextFactory<ReadOnlyDbContext> factory,
        IReadOnlySelector selector)
    {
        _factory = factory;
        _selector = selector;
    }

    public async Task<DbTest> Go(CancellationToken cancellationToken = default)
    {
        // Hent connectionstring HER
        var conn = _selector.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseNpgsql(conn, npgsql =>
        {
            npgsql.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(2),
                errorCodesToAdd: null);
        })
        .Options;

        await using var db = new ReadOnlyDbContext(options);

        var result = await db.Database.SqlQueryRaw<DbTest>(
            """
            SELECT 
                inet_server_addr()::text AS "IP",
                current_database()::text AS "DB",
                COALESCE(current_setting('primary_slot_name', true), 'no-slot') AS "Slot"
            """
        ).FirstOrDefaultAsync(cancellationToken);

        return result!;
    }
}


public class DbTest
{
    public string IP { get; set; } = string.Empty;
    public string DB { get; set; } = string.Empty;
    public string Slot { get; set; } = string.Empty;
}
