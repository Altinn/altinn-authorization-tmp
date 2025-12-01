using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

public class AppReplicaDbContext(DbContextOptions<AppReplicaDbContext> options) : DbContext(options)
{
    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<AssignmentPackage> AssignmentPackages => Set<AssignmentPackage>();

    public DbSet<DelegationPackage> DelegationPackages => Set<DelegationPackage>();

    public DbSet<AssignmentResource> AssignmentResources => Set<AssignmentResource>();

    public DbSet<RoleResource> RoleResources => Set<RoleResource>();

    public DbSet<DelegationResource> DelegationResources => Set<DelegationResource>();

    public DbSet<RolePackage> RolePackages => Set<RolePackage>();

    public DbSet<Package> Packages => Set<Package>();

    public DbSet<RoleMap> RoleMaps => Set<RoleMap>();

    public DbSet<Entity> Entities => Set<Entity>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<Delegation> Delegations => Set<Delegation>();

    public DbSet<PackageResource> PackageResources => Set<PackageResource>();

    public DbSet<Resource> Resources => Set<Resource>();

    public override int SaveChanges()
        => throw new InvalidOperationException("Read-only context - SaveChanges is not allowed.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("Read-only context - SaveChangesAsync is not allowed.");
}
