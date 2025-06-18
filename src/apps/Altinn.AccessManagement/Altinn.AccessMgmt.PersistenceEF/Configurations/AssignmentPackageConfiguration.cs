using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class AssignmentPackageConfiguration : IEntityTypeConfiguration<AssignmentPackage> {
    public void Configure(EntityTypeBuilder<AssignmentPackage> builder)
    {
        builder.ToTable("AssignmentPackage", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.AssignmentId).IsRequired();
        builder.Property(t => t.PackageId).IsRequired();

        builder.HasIndex(t => new { t.AssignmentId, t.PackageId }).IsUnique();
    }
}

public class ExtendedAssignmentPackageConfiguration : IEntityTypeConfiguration<ExtAssignmentPackage> {
    public void Configure(EntityTypeBuilder<ExtAssignmentPackage> builder)
    {
        builder.ToTable("AssignmentPackage", "dbo");
        builder.HasOne(p => p.Assignment).WithMany().HasForeignKey(p => p.AssignmentId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Package).WithMany().HasForeignKey(p => p.PackageId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditAssignmentPackageConfiguration : AuditConfiguration<AuditAssignmentPackage> { public AuditAssignmentPackageConfiguration() : base("AssignmentPackage") { } }
