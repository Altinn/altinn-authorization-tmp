using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <summary>
/// Used by cli `dotnet ef migration`
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("APPDB_CONNECTION") ?? string.Empty;

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .ReplaceService<IMigrationsSqlGenerator, CustomMigrationsSqlGenerator>()
            .Options;

        return new AppDbContext(options, new DesignTimeAuditAccessor());
    }
}
