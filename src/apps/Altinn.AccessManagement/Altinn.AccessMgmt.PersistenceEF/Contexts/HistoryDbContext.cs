using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

public class HistoryDbContext : DbContext
{
    public HistoryDbContext(DbContextOptions<HistoryDbContext> options) : base(options) { }

    public DbSet<AuditPackage> PackageHistorys => Set<AuditPackage>();
    //public DbSet<RoleHistory> RoleHistorys => Set<RoleHistory>();
    //public DbSet<CategoryHistory> CategoryHistorys => Set<CategoryHistory>();
    //public DbSet<RolePackageHistory> RolePackageHistorys => Set<RolePackageHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PackageHistoryConfiguration());
        //modelBuilder.ApplyConfiguration(new RoleHistoryConfiguration());
        //modelBuilder.ApplyConfiguration(new CategoryHistoryConfiguration());
        //modelBuilder.ApplyConfiguration(new RolePackageHistoryConfiguration());
    }
}
