using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public partial class ExtendedDbContext : DbContext
{
    public ExtendedDbContext(DbContextOptions<ExtendedDbContext> options) : base(options) { }

    #region DbSets
    public DbSet<ExtArea> Areas => Set<ExtArea>();

    public DbSet<ExtAreaGroup> AreaGroups => Set<ExtAreaGroup>();

    public DbSet<ExtAssignment> Assignments => Set<ExtAssignment>();

    public DbSet<ExtAssignmentPackage> AssignmentPackages => Set<ExtAssignmentPackage>();

    public DbSet<ExtAssignmentResource> AssignmentResources => Set<ExtAssignmentResource>();

    public DbSet<ExtConnection> Connections => Set<ExtConnection>();

    public DbSet<ExtConnectionPackage> ConnectionPackages => Set<ExtConnectionPackage>();

    public DbSet<ExtConnectionResource> ConnectionResources => Set<ExtConnectionResource>();

    public DbSet<ExtDelegation> Delegations => Set<ExtDelegation>();

    public DbSet<ExtDelegationPackage> DelegationPackages => Set<ExtDelegationPackage>();

    public DbSet<ExtDelegationResource> DelegationResources => Set<ExtDelegationResource>();

    public DbSet<ExtEntity> Entities => Set<ExtEntity>();

    public DbSet<ExtEntityLookup> EntityLookups => Set<ExtEntityLookup>();

    public DbSet<ExtEntityType> EntityTypes => Set<ExtEntityType>();

    public DbSet<ExtEntityVariant> EntityVariants => Set<ExtEntityVariant>();

    public DbSet<ExtEntityVariantRole> EntityVariantRoles => Set<ExtEntityVariantRole>();

    public DbSet<ExtPackage> Packages => Set<ExtPackage>();

    public DbSet<ExtPackageResource> PackageResources => Set<ExtPackageResource>();

    public DbSet<ExtProvider> Providers => Set<ExtProvider>();

    public DbSet<ExtRelation> Relations => Set<ExtRelation>();

    public DbSet<ExtResource> Resources => Set<ExtResource>();

    public DbSet<ExtRole> Roles => Set<ExtRole>();

    public DbSet<ExtRoleLookup> RoleLookups => Set<ExtRoleLookup>();

    public DbSet<ExtRoleMap> RoleMaps => Set<ExtRoleMap>();

    public DbSet<ExtRolePackage> RolePackages => Set<ExtRolePackage>();

    public DbSet<ExtRoleResource> RoleResources => Set<ExtRoleResource>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration<ExtArea>(new ExtendedAreaConfiguration());
        modelBuilder.ApplyConfiguration<ExtAreaGroup>(new ExtendedAreaGroupConfiguration());
        modelBuilder.ApplyConfiguration<ExtAssignment>(new ExtendedAssignmentConfiguration());
        modelBuilder.ApplyConfiguration<ExtAssignmentPackage>(new ExtendedAssignmentPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtAssignmentResource>(new ExtendedAssignmentResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtConnection>(new ExtendedConnectionConfiguration());
        modelBuilder.ApplyConfiguration<ExtConnectionPackage>(new ExtendedConnectionPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtConnectionResource>(new ExtendedConnectionResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtDelegation>(new ExtendedDelegationConfiguration());
        modelBuilder.ApplyConfiguration<ExtDelegationPackage>(new ExtendedDelegationPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtDelegationResource>(new ExtendedDelegationResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtEntity>(new ExtendedEntityConfiguration());
        modelBuilder.ApplyConfiguration<ExtEntityLookup>(new ExtendedEntityLookupConfiguration());
        modelBuilder.ApplyConfiguration<ExtEntityType>(new ExtendedEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration<ExtEntityVariant>(new ExtendedEntityVariantConfiguration());
        modelBuilder.ApplyConfiguration<ExtEntityVariantRole>(new ExtendedEntityVariantRoleConfiguration());
        modelBuilder.ApplyConfiguration<ExtPackage>(new ExtendedPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtPackageResource>(new ExtendedPackageResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtProvider>(new ExtendedProviderConfiguration());
        modelBuilder.ApplyConfiguration<ExtRelation>(new ExtendedRelationConfiguration());
        modelBuilder.ApplyConfiguration<ExtResource>(new ExtendedResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtRole>(new ExtendedRoleConfiguration());
        modelBuilder.ApplyConfiguration<ExtRoleLookup>(new ExtendedRoleLookupConfiguration());
        modelBuilder.ApplyConfiguration<ExtRoleMap>(new ExtendedRoleMapConfiguration());
        modelBuilder.ApplyConfiguration<ExtRolePackage>(new ExtendedRolePackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtRoleResource>(new ExtendedRoleResourceConfiguration());
    }
}
