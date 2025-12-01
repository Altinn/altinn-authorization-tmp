using Altinn.AccessMgmt.PersistenceEF.Audit;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class AppPrimaryDbContextFactory(IDbContextFactory<AppPrimaryDbContext> factory, IAuditAccessor audit) : IDbContextFactory<AppPrimaryDbContext>
{
    public AppPrimaryDbContext CreateDbContext()
    {
        var dbContext = factory.CreateDbContext();
        dbContext.AuditAccessor = audit;
        return dbContext;
    }
}
