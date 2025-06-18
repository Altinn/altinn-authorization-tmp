using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RoleMapConfiguration : IEntityTypeConfiguration<RoleMap> {
    public void Configure(EntityTypeBuilder<RoleMap> builder)
    {
        builder.ToTable("RoleMap", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.HasRoleId).IsRequired();
        builder.Property(t => t.GetRoleId).IsRequired();

        builder.HasIndex(t => new { t.HasRoleId, t.GetRoleId }).IsUnique();
    }
}

public class ExtendedRoleMapConfiguration : IEntityTypeConfiguration<ExtRoleMap> {
    public void Configure(EntityTypeBuilder<ExtRoleMap> builder)
    {
        builder.ToTable("RoleMap", "dbo");
        builder.HasOne(p => p.HasRole).WithMany().HasForeignKey(p => p.HasRoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.GetRole).WithMany().HasForeignKey(p => p.GetRoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditRoleMapConfiguration : AuditConfiguration<AuditRoleMap> { public AuditRoleMapConfiguration() : base("RoleMap") { } }
