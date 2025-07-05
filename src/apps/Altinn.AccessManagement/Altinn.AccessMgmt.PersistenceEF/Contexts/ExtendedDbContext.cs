using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class ExtendedDbContext : DbContext
{
    public ExtendedDbContext(DbContextOptions<ExtendedDbContext> options) : base(options) { }

    #region ExtendedDbSets

    public DbSet<ExtendedArea> ExtendedAreas => Set<ExtendedArea>();

    public DbSet<ExtendedAreaGroup> ExtendedAreaGroups => Set<ExtendedAreaGroup>();

    public DbSet<ExtendedAssignment> ExtendedAssignments => Set<ExtendedAssignment>();

    public DbSet<ExtendedAssignmentPackage> ExtendedAssignmentPackages => Set<ExtendedAssignmentPackage>();

    public DbSet<ExtendedAssignmentResource> ExtendedAssignmentResources => Set<ExtendedAssignmentResource>();

    //public DbSet<ExtendedConnection> ExtendedConnections => Set<ExtendedConnection>();

    //public DbSet<ExtendedConnectionPackage> ExtendedConnectionPackages => Set<ExtendedConnectionPackage>();

    //public DbSet<ExtendedConnectionResource> ExtendedConnectionResources => Set<ExtendedConnectionResource>();

    public DbSet<ExtendedDelegation> ExtendedDelegations => Set<ExtendedDelegation>();

    public DbSet<ExtendedDelegationPackage> ExtendedDelegationPackages => Set<ExtendedDelegationPackage>();

    public DbSet<ExtendedDelegationResource> ExtendedDelegationResources => Set<ExtendedDelegationResource>();

    public DbSet<ExtendedEntity> ExtendedEntities => Set<ExtendedEntity>();

    public DbSet<ExtendedEntityLookup> ExtendedEntityLookups => Set<ExtendedEntityLookup>();

    public DbSet<ExtendedEntityType> ExtendedEntityTypes => Set<ExtendedEntityType>();

    public DbSet<ExtendedEntityVariant> ExtendedEntityVariants => Set<ExtendedEntityVariant>();

    public DbSet<ExtendedEntityVariantRole> ExtendedEntityVariantRoles => Set<ExtendedEntityVariantRole>();

    public DbSet<ExtendedPackage> ExtendedPackages => Set<ExtendedPackage>();

    public DbSet<ExtendedPackageResource> ExtendedPackageResources => Set<ExtendedPackageResource>();

    public DbSet<ExtendedProvider> ExtendedProviders => Set<ExtendedProvider>();

    //public DbSet<ExtendedRelation> ExtendedRelations => Set<ExtendedRelation>();

    public DbSet<ExtendedResource> ExtendedResources => Set<ExtendedResource>();

    public DbSet<ExtendedRole> ExtendedRoles => Set<ExtendedRole>();

    public DbSet<ExtendedRoleLookup> ExtendedRoleLookups => Set<ExtendedRoleLookup>();

    public DbSet<ExtendedRoleMap> ExtendedRoleMaps => Set<ExtendedRoleMap>();

    public DbSet<ExtendedRolePackage> ExtendedRolePackages => Set<ExtendedRolePackage>();

    public DbSet<ExtendedRoleResource> ExtendedRoleResources => Set<ExtendedRoleResource>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ApplyExtendedConfiguration(modelBuilder);

        modelBuilder.UseLowerCaseNamingConvention();
    }

    private void ApplyExtendedConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration<ExtendedArea>(new ExtendedAreaConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedAreaGroup>(new ExtendedAreaGroupConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedAssignment>(new ExtendedAssignmentConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedAssignmentPackage>(new ExtendedAssignmentPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedAssignmentResource>(new ExtendedAssignmentResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedDelegation>(new ExtendedDelegationConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedDelegationPackage>(new ExtendedDelegationPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedDelegationResource>(new ExtendedDelegationResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedEntity>(new ExtendedEntityConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedEntityLookup>(new ExtendedEntityLookupConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedEntityType>(new ExtendedEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedEntityVariant>(new ExtendedEntityVariantConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedEntityVariantRole>(new ExtendedEntityVariantRoleConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedPackage>(new ExtendedPackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedPackageResource>(new ExtendedPackageResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedProvider>(new ExtendedProviderConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedResource>(new ExtendedResourceConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedRole>(new ExtendedRoleConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedRoleLookup>(new ExtendedRoleLookupConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedRoleMap>(new ExtendedRoleMapConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedRolePackage>(new ExtendedRolePackageConfiguration());
        modelBuilder.ApplyConfiguration<ExtendedRoleResource>(new ExtendedRoleResourceConfiguration());
    }
}
