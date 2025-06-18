using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RoleResourceConfiguration : IEntityTypeConfiguration<RoleResource> {
    public void Configure(EntityTypeBuilder<RoleResource> builder)
    {
        builder.ToTable("RoleResource", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RoleId).IsRequired();
        builder.Property(t => t.ResourceId).IsRequired();

        builder.HasIndex(t => new { t.RoleId, t.ResourceId }).IsUnique();
    }
}
public class ExtendedRoleResourceConfiguration : IEntityTypeConfiguration<ExtRoleResource> {
    public void Configure(EntityTypeBuilder<ExtRoleResource> builder)
    {
        builder.ToTable("RoleResource", "dbo");
        builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Resource).WithMany().HasForeignKey(p => p.ResourceId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}
public class AuditRoleResourceConfiguration : AuditConfiguration<AuditRoleResource> { public AuditRoleResourceConfiguration() : base("RoleResource") { } }
