using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class BasicDbContext : DbContext
{
    public BasicDbContext(DbContextOptions<BasicDbContext> options) : base(options) { }

    #region DbSets
    public DbSet<Area> Areas => Set<Area>();

    public DbSet<AreaGroup> AreaGroups => Set<AreaGroup>();

    public DbSet<Assignment> Assignments => Set<Assignment>();

    public DbSet<AssignmentPackage> AssignmentPackages => Set<AssignmentPackage>();

    public DbSet<AssignmentResource> AssignmentResources => Set<AssignmentResource>();

    public DbSet<Connection> Connections => Set<Connection>();

    public DbSet<ConnectionPackage> ConnectionPackages => Set<ConnectionPackage>();

    public DbSet<ConnectionResource> ConnectionResources => Set<ConnectionResource>();

    public DbSet<Delegation> Delegations => Set<Delegation>();

    public DbSet<DelegationPackage> DelegationPackages => Set<DelegationPackage>();

    public DbSet<DelegationResource> DelegationResources => Set<DelegationResource>();

    public DbSet<Entity> Entities => Set<Entity>();

    public DbSet<EntityLookup> EntityLookups => Set<EntityLookup>();

    public DbSet<EntityType> EntityTypes => Set<EntityType>();

    public DbSet<EntityVariant> EntityVariants => Set<EntityVariant>();

    public DbSet<EntityVariantRole> EntityVariantRoles => Set<EntityVariantRole>();

    public DbSet<Package> Packages => Set<Package>();

    public DbSet<PackageResource> PackageResources => Set<PackageResource>();

    public DbSet<Provider> Providers => Set<Provider>();

    public DbSet<ProviderType> ProviderTypes => Set<ProviderType>();

    public DbSet<Relation> Relations => Set<Relation>();

    public DbSet<Resource> Resources => Set<Resource>();

    public DbSet<ResourceType> ResourceTypes => Set<ResourceType>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<RoleLookup> RoleLookups => Set<RoleLookup>();

    public DbSet<RoleMap> RoleMaps => Set<RoleMap>();

    public DbSet<RolePackage> RolePackages => Set<RolePackage>();

    public DbSet<RoleResource> RoleResources => Set<RoleResource>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration<Area>(new AreaConfiguration());
        modelBuilder.ApplyConfiguration<AreaGroup>(new AreaGroupConfiguration());
        modelBuilder.ApplyConfiguration<Assignment>(new AssignmentConfiguration());
        modelBuilder.ApplyConfiguration<AssignmentPackage>(new AssignmentPackageConfiguration());
        modelBuilder.ApplyConfiguration<AssignmentResource>(new AssignmentResourceConfiguration());
        modelBuilder.ApplyConfiguration<Connection>(new ConnectionConfiguration());
        modelBuilder.ApplyConfiguration<ConnectionPackage>(new ConnectionPackageConfiguration());
        modelBuilder.ApplyConfiguration<ConnectionResource>(new ConnectionResourceConfiguration());
        modelBuilder.ApplyConfiguration<Delegation>(new DelegationConfiguration());
        modelBuilder.ApplyConfiguration<DelegationPackage>(new DelegationPackageConfiguration());
        modelBuilder.ApplyConfiguration<DelegationResource>(new DelegationResourceConfiguration());
        modelBuilder.ApplyConfiguration<Entity>(new EntityConfiguration());
        modelBuilder.ApplyConfiguration<EntityLookup>(new EntityLookupConfiguration());
        modelBuilder.ApplyConfiguration<EntityType>(new EntityTypeConfiguration());
        modelBuilder.ApplyConfiguration<EntityVariant>(new EntityVariantConfiguration());
        modelBuilder.ApplyConfiguration<EntityVariantRole>(new EntityVariantRoleConfiguration());
        modelBuilder.ApplyConfiguration<Package>(new PackageConfiguration());
        modelBuilder.ApplyConfiguration<PackageResource>(new PackageResourceConfiguration());
        modelBuilder.ApplyConfiguration<Provider>(new ProviderConfiguration());
        modelBuilder.ApplyConfiguration<ProviderType>(new ProviderTypeConfiguration());
        modelBuilder.ApplyConfiguration<Relation>(new RelationConfiguration());
        modelBuilder.ApplyConfiguration<Resource>(new ResourceConfiguration());
        modelBuilder.ApplyConfiguration<ResourceType>(new ResourceTypeConfiguration());
        modelBuilder.ApplyConfiguration<Role>(new RoleConfiguration());
        modelBuilder.ApplyConfiguration<RoleLookup>(new RoleLookupConfiguration());
        modelBuilder.ApplyConfiguration<RoleMap>(new RoleMapConfiguration());
        modelBuilder.ApplyConfiguration<RolePackage>(new RolePackageConfiguration());
        modelBuilder.ApplyConfiguration<RoleResource>(new RoleResourceConfiguration());
    }
}
