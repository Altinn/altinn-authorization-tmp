using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.ConsoleTester;

public class ReadOnlyRoundRobinTester(IDbContextFactory<ReadOnlyDbContext> factory)
{
    public async Task<DbTest> Go()
    {
        using var db = factory.CreateDbContext();
        return await db.Database.SqlQueryRaw<DbTest>("SELECT inet_server_addr()::text as \"IP\", current_database()::text as \"DB\", current_setting('primary_slot_name') as \"Slot\" ").FirstOrDefaultAsync();
    }
}

public class DbTest
{
    public string IP { get; set; }

    public string DB { get; set; }

    public string Slot { get; set; }
}
