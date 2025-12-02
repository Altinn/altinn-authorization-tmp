using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class ReadOnlyDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options) 
{
    public override int SaveChanges()
    => throw new InvalidOperationException("Read-only context - SaveChanges is not allowed.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    => throw new InvalidOperationException("Read-only context - SaveChangesAsync is not allowed.");
}

public class ReadOnlyDbContextContextFactory(IDbContextFactory<ReadOnlyDbContext> factory) : IDbContextFactory<ReadOnlyDbContext>
{
    public ReadOnlyDbContext CreateDbContext()
    {
        var dbContext = factory.CreateDbContext();
        return dbContext;
    }
}
