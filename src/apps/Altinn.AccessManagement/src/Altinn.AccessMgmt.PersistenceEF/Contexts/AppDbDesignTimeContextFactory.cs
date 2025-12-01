using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <summary>
/// Used by cli `dotnet ef migration`
/// </summary>
public sealed class AppDbDesignTimeContextFactory : IDesignTimeDbContextFactory<AppPrimaryDbContext>
{
    public AppPrimaryDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("Database:Postgres:MigrationConnectionString") ?? "Database=accessmgmt_ef_02;Host=localhost;Username=platform_authorization_admin;Password=Password;Include Error Detail=true";

        var options = new DbContextOptionsBuilder<AppPrimaryDbContext>()
            .UseNpgsql(cs)
            .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>()
            .Options;

        return new AppPrimaryDbContext(options)
        {
            AuditAccessor = new AuditAccessor()
            {
                AuditValues = new AuditValues(Guid.Empty, Guid.Empty, "design-time", DateTimeOffset.UtcNow),
            }
        };
    }
}
