using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RoleLookupConfiguration : IEntityTypeConfiguration<RoleLookup>
{
    public void Configure(EntityTypeBuilder<RoleLookup> builder)
    {
        builder.ToTable("RoleLookup", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RoleId).IsRequired();
        builder.Property(t => t.Key).IsRequired();
        builder.Property(t => t.Value).IsRequired();

        builder.HasIndex(t => new { t.RoleId, t.Key }).IncludeProperties(p => new { p.Value, p.Id }).IsUnique();
    }
}

public class ExtendedRoleLookupConfiguration : IEntityTypeConfiguration<ExtendedRoleLookup> {
    public void Configure(EntityTypeBuilder<ExtendedRoleLookup> builder)
    {
        builder.ToTable("RoleLookup", "dbo");
        builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditRoleLookupConfiguration : AuditConfiguration<AuditRoleLookup> { public AuditRoleLookupConfiguration() : base("RoleLookup") { } }
