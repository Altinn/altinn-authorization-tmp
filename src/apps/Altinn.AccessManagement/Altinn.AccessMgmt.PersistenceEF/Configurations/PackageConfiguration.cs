using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("package", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name);
        builder.Property(t => t.Description);
        builder.Property(t => t.Urn);
        builder.Property(t => t.IsAssignable).HasDefaultValue(false);
        builder.Property(t => t.IsDelegable).HasDefaultValue(false);
        builder.Property(t => t.HasResources);
        builder.Property(t => t.ProviderId);
        builder.Property(t => t.EntityTypeId);
        builder.Property(t => t.AreaId);

        builder.HasIndex(t => new { t.ProviderId, t.Name }).IsUnique().HasDatabaseName("UC_dbo_Package__Provider_Name");
    }
}

public class ExtendedPackageConfiguration : IEntityTypeConfiguration<ExtPackage>
{
    public void Configure(EntityTypeBuilder<ExtPackage> builder)
    {
        builder.ToTable("package", "dbo");
        builder.HasOne(p => p.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.EntityType).WithMany().HasForeignKey(p => p.EntityTypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(p => p.Area).WithMany().HasForeignKey(p => p.AreaId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.NoAction);
    }
}

public class AuditPackageConfiguration : AuditConfiguration<AuditPackage>
{
    public AuditPackageConfiguration() : base("package") { }
}
/// <inheritdoc />
public abstract partial class BasicDbContext : DbContext
{
    public DbSet<Package> Packages => Set<Package>();

    public DbSet<ExtPackage> ExtendedPackages => Set<ExtPackage>();
}

/// <inheritdoc />
public partial class AuditDbContext : DbContext
{
    public DbSet<AuditPackage> AuditPackages => Set<AuditPackage>();
}
