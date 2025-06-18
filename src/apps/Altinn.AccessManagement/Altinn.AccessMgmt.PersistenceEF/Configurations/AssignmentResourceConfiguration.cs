using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class AssignmentResourceConfiguration : IEntityTypeConfiguration<AssignmentResource> {
    public void Configure(EntityTypeBuilder<AssignmentResource> builder)
    {
        builder.ToTable("AssignmentResource", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.AssignmentId).IsRequired();
        builder.Property(t => t.ResourceId).IsRequired();

        builder.HasIndex(t => new { t.AssignmentId, t.ResourceId }).IsUnique();
    }
}

public class ExtendedAssignmentResourceConfiguration : IEntityTypeConfiguration<ExtAssignmentResource> {
    public void Configure(EntityTypeBuilder<ExtAssignmentResource> builder)
    {
        builder.ToTable("AssignmentResource", "dbo");
        builder.HasOne(p => p.Assignment).WithMany().HasForeignKey(p => p.AssignmentId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Resource).WithMany().HasForeignKey(p => p.ResourceId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditAssignmentResourceConfiguration : AuditConfiguration<AuditAssignmentResource> { public AuditAssignmentResourceConfiguration() : base("AssignmentResource") { } }
