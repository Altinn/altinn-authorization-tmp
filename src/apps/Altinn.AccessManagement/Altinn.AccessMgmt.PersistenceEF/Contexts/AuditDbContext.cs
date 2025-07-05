using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.PersistenceEF.Contexts;

/// <inheritdoc />
public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }

    #region DbSets
    public DbSet<AuditArea> Areas => Set<AuditArea>();

    public DbSet<AuditAreaGroup> AreaGroups => Set<AuditAreaGroup>();

    public DbSet<AuditAssignment> Assignments => Set<AuditAssignment>();

    public DbSet<AuditAssignmentPackage> AssignmentPackages => Set<AuditAssignmentPackage>();

    public DbSet<AuditAssignmentResource> AssignmentResources => Set<AuditAssignmentResource>();

    //public DbSet<AuditConnection> Connections => Set<AuditConnection>();

    //public DbSet<AuditConnectionPackage> ConnectionPackages => Set<AuditConnectionPackage>();

    //public DbSet<AuditConnectionResource> ConnectionResources => Set<AuditConnectionResource>();

    public DbSet<AuditDelegation> Delegations => Set<AuditDelegation>();

    public DbSet<AuditDelegationPackage> DelegationPackages => Set<AuditDelegationPackage>();

    public DbSet<AuditDelegationResource> DelegationResources => Set<AuditDelegationResource>();

    public DbSet<AuditEntity> Entities => Set<AuditEntity>();

    public DbSet<AuditEntityLookup> EntityLookups => Set<AuditEntityLookup>();

    public DbSet<AuditEntityType> EntityTypes => Set<AuditEntityType>();

    public DbSet<AuditEntityVariant> EntityVariants => Set<AuditEntityVariant>();

    public DbSet<AuditEntityVariantRole> EntityVariantRoles => Set<AuditEntityVariantRole>();

    public DbSet<AuditPackage> Packages => Set<AuditPackage>();

    public DbSet<AuditPackageResource> PackageResources => Set<AuditPackageResource>();

    public DbSet<AuditProvider> Providers => Set<AuditProvider>();

    public DbSet<AuditProviderType> ProviderTypes => Set<AuditProviderType>();

    //public DbSet<AuditRelation> Relations => Set<AuditRelation>();

    public DbSet<AuditResource> Resources => Set<AuditResource>();

    public DbSet<AuditResourceType> ResourceTypes => Set<AuditResourceType>();

    public DbSet<AuditRole> Roles => Set<AuditRole>();

    public DbSet<AuditRoleLookup> RoleLookups => Set<AuditRoleLookup>();

    public DbSet<AuditRoleMap> RoleMaps => Set<AuditRoleMap>();

    public DbSet<AuditRolePackage> RolePackages => Set<AuditRolePackage>();

    public DbSet<AuditRoleResource> RoleResources => Set<AuditRoleResource>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration<AuditArea>(new AuditAreaConfiguration());
        modelBuilder.ApplyConfiguration<AuditAreaGroup>(new AuditAreaGroupConfiguration());
        modelBuilder.ApplyConfiguration<AuditAssignment>(new AuditAssignmentConfiguration());
        modelBuilder.ApplyConfiguration<AuditAssignmentPackage>(new AuditAssignmentPackageConfiguration());
        modelBuilder.ApplyConfiguration<AuditAssignmentResource>(new AuditAssignmentResourceConfiguration());
        //modelBuilder.ApplyConfiguration<AuditConnection>(new AuditConnectionConfiguration());
        //modelBuilder.ApplyConfiguration<AuditConnectionPackage>(new AuditConnectionPackageConfiguration());
        //modelBuilder.ApplyConfiguration<AuditConnectionResource>(new AuditConnectionResourceConfiguration());
        modelBuilder.ApplyConfiguration<AuditDelegation>(new AuditDelegationConfiguration());
        modelBuilder.ApplyConfiguration<AuditDelegationPackage>(new AuditDelegationPackageConfiguration());
        modelBuilder.ApplyConfiguration<AuditDelegationResource>(new AuditDelegationResourceConfiguration());
        modelBuilder.ApplyConfiguration<AuditEntity>(new AuditEntityConfiguration());
        modelBuilder.ApplyConfiguration<AuditEntityLookup>(new AuditEntityLookupConfiguration());
        modelBuilder.ApplyConfiguration<AuditEntityType>(new AuditEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration<AuditEntityVariant>(new AuditEntityVariantConfiguration());
        modelBuilder.ApplyConfiguration<AuditEntityVariantRole>(new AuditEntityVariantRoleConfiguration());
        modelBuilder.ApplyConfiguration<AuditPackage>(new AuditPackageConfiguration());
        modelBuilder.ApplyConfiguration<AuditPackageResource>(new AuditPackageResourceConfiguration());
        modelBuilder.ApplyConfiguration<AuditProvider>(new AuditProviderConfiguration());
        modelBuilder.ApplyConfiguration<AuditProviderType>(new AuditProviderTypeConfiguration());
        //modelBuilder.ApplyConfiguration<AuditRelation>(new AuditRelationConfiguration());
        modelBuilder.ApplyConfiguration<AuditResource>(new AuditResourceConfiguration());
        modelBuilder.ApplyConfiguration<AuditResourceType>(new AuditResourceTypeConfiguration());
        modelBuilder.ApplyConfiguration<AuditRole>(new AuditRoleConfiguration());
        modelBuilder.ApplyConfiguration<AuditRoleLookup>(new AuditRoleLookupConfiguration());
        modelBuilder.ApplyConfiguration<AuditRoleMap>(new AuditRoleMapConfiguration());
        modelBuilder.ApplyConfiguration<AuditRolePackage>(new AuditRolePackageConfiguration());
        modelBuilder.ApplyConfiguration<AuditRoleResource>(new AuditRoleResourceConfiguration());
    }
}
