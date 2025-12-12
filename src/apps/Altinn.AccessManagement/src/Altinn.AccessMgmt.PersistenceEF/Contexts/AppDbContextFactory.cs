using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy.Enums;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class AppDbContextFactory(IDbContextFactory<AppDbContext> factory, IAuditAccessor audit) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        var cs = Environment.GetEnvironmentVariable("Database:Postgres:MigrationConnectionString") ?? "Database=accessmgmt_ef_02;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs, opt => 
            {
                opt.MapEnum<UuidType>("delegation", nameof(UuidType).ToLower());
                opt.MapEnum<DelegationChangeType>("delegation", nameof(DelegationChangeType).ToLower());
            })
            .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>()
            .Options;

        return new AppDbContext(options, new DesignTimeAuditAccessor());
        
        var dbContext = factory.CreateDbContext();
        dbContext.AuditAccessor = audit;
        return dbContext;
    }
}
