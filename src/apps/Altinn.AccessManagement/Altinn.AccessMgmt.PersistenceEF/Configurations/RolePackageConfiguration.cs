using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RolePackageConfiguration : IEntityTypeConfiguration<RolePackage>
{
    public void Configure(EntityTypeBuilder<RolePackage> builder)
    {
        builder.ToTable("RolePackage", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RoleId).IsRequired();
        builder.Property(t => t.PackageId).IsRequired();
        builder.Property(t => t.HasAccess).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.CanDelegate).IsRequired().HasDefaultValue(false);
        builder.Property(t => t.EntityVariantId);

        builder.HasIndex(t => new { t.RoleId, t.PackageId }).IncludeProperties(t => t.EntityVariantId).IsUnique();
    }
}

public class ExtendedRolePackageConfiguration : IEntityTypeConfiguration<ExtendedRolePackage> {
    public void Configure(EntityTypeBuilder<ExtendedRolePackage> builder)
    {
        builder.ToTable("RolePackage", "dbo");
        builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Package).WithMany().HasForeignKey(p => p.PackageId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.EntityVariant).WithMany().HasForeignKey(p => p.EntityVariantId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditRolePackageConfiguration : AuditConfiguration<AuditRolePackage> { public AuditRolePackageConfiguration() : base("RolePackage") { } }
