using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role> {
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Code).IsRequired();
        builder.Property(t => t.Urn).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.IsKeyRole).HasDefaultValue(false);
        builder.Property(t => t.IsAssignable).HasDefaultValue(false);
        builder.Property(t => t.ProviderId).IsRequired();
        builder.Property(t => t.EntityTypeId).IsRequired();

        builder.HasIndex(t => t.Urn).IsUnique();
        builder.HasIndex(t => new { t.ProviderId, t.Name }).IsUnique();
        builder.HasIndex(t => new { t.ProviderId, t.Code }).IsUnique();
    }
}

public class ExtendedRoleConfiguration : IEntityTypeConfiguration<ExtendedRole> {
    public void Configure(EntityTypeBuilder<ExtendedRole> builder)
    {
        builder.ToTable("Role", "dbo");
        builder.HasOne(p => p.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.EntityType).WithMany().HasForeignKey(p => p.EntityTypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditRoleConfiguration : AuditConfiguration<AuditRole> { public AuditRoleConfiguration() : base("Role") { } }
