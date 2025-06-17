using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Package> Packages => Set<Package>();
    //public DbSet<Role> Roles => Set<Role>();
    //public DbSet<RolePackage> RolePackages => Set<RolePackage>();
    //public DbSet<ExtPackage> ExtendedPackages => Set<ExtPackage>();
    //public DbSet<ExtRole> ExtendedRoles => Set<ExtRole>();
    //public DbSet<ExtRolePackage> ExtendedRolePackages => Set<ExtRolePackage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PackageAppConfiguration());
        //modelBuilder.ApplyConfiguration(new RoleAppConfiguration());
        //modelBuilder.ApplyConfiguration(new CategoryAppConfiguration());
        //modelBuilder.ApplyConfiguration(new RolePackageAppConfiguration());
        //modelBuilder.ApplyConfiguration(new PackageExtendedConfiguration());
        //modelBuilder.ApplyConfiguration(new RoleExtendedConfiguration());
        //modelBuilder.ApplyConfiguration(new RolePackageExtendedConfiguration());
    }
}
