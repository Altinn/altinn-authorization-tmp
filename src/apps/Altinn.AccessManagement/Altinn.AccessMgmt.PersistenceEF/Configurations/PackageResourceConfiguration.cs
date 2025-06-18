using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class PackageResourceConfiguration : IEntityTypeConfiguration<PackageResource> {
    public void Configure(EntityTypeBuilder<PackageResource> builder)
    {
        builder.ToTable("PackageResource", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.PackageId).IsRequired();
        builder.Property(t => t.ResourceId).IsRequired();

        builder.HasIndex(t => new { t.PackageId, t.ResourceId }).IsUnique();
    }
}

public class ExtendedPackageResourceConfiguration : IEntityTypeConfiguration<ExtPackageResource> {
    public void Configure(EntityTypeBuilder<ExtPackageResource> builder)
    {
        builder.ToTable("PackageResource", "dbo");
        builder.HasOne(p => p.Package).WithMany().HasForeignKey(p => p.PackageId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Resource).WithMany().HasForeignKey(p => p.ResourceId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditPackageResourceConfiguration : AuditConfiguration<AuditPackageResource> { public AuditPackageResourceConfiguration() : base("PackageResource") { } }
