using Altinn.AccessMgmt.PersistenceEF.Audit;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class AppReplicaDbContextFactory(IDbContextFactory<AppReplicaDbContext> factory) : IDbContextFactory<AppReplicaDbContext>
{
    public AppReplicaDbContext CreateDbContext()
    {
        var dbContext = factory.CreateDbContext();
        return dbContext;
    }
}
