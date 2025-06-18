using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignment", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.FromId).IsRequired();
        builder.Property(t => t.ToId).IsRequired();
        builder.Property(t => t.RoleId).IsRequired();

        builder.HasIndex(t => new { t.FromId, t.ToId, t.RoleId }).IsUnique();
    }
}

public class ExtendedAssignmentConfiguration : IEntityTypeConfiguration<ExtAssignment> {
    public void Configure(EntityTypeBuilder<ExtAssignment> builder)
    {
        builder.ToTable("assignment", "dbo");
        builder.HasOne(p => p.From).WithMany().HasForeignKey(p => p.FromId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.To).WithMany().HasForeignKey(p => p.ToId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditAssignmentConfiguration : AuditConfiguration<AuditAssignment> { public AuditAssignmentConfiguration() : base("assignment") { } }
