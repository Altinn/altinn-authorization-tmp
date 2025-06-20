using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class EntityVariantRoleConfiguration : IEntityTypeConfiguration<EntityVariantRole> {
    public void Configure(EntityTypeBuilder<EntityVariantRole> builder)
    {
        builder.ToTable("EntityVariantRole", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.VariantId).IsRequired();
        builder.Property(t => t.RoleId).IsRequired();

        builder.HasIndex(t => new { t.VariantId, t.RoleId }).IsUnique();
    }
}

public class ExtendedEntityVariantRoleConfiguration : IEntityTypeConfiguration<ExtendedEntityVariantRole>
{
    public void Configure(EntityTypeBuilder<ExtendedEntityVariantRole> builder)
    {
        builder.ToTable("EntityVariantRole", "dbo");
        builder.HasOne(p => p.Variant).WithMany().HasForeignKey(p => p.VariantId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Role).WithMany().HasForeignKey(p => p.RoleId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditEntityVariantRoleConfiguration : AuditConfiguration<AuditEntityVariantRole> { public AuditEntityVariantRoleConfiguration() : base("EntityVariantRole") { } }
