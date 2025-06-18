using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class EntityConfiguration : IEntityTypeConfiguration<Entity> {
    public void Configure(EntityTypeBuilder<Entity> builder)
    {
        builder.ToTable("entity", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.RefId);
        builder.Property(t => t.TypeId).IsRequired();
        builder.Property(t => t.VariantId).IsRequired();
        builder.Property(t => t.ParentId);

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedEntityConfiguration : IEntityTypeConfiguration<ExtEntity> {
    public void Configure(EntityTypeBuilder<ExtEntity> builder)
    {
        builder.ToTable("entity", "dbo");
        builder.HasOne(p => p.Type).WithMany().HasForeignKey(p => p.TypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Variant).WithMany().HasForeignKey(p => p.VariantId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Parent).WithMany().HasForeignKey(p => p.ParentId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AuditEntityConfiguration : AuditConfiguration<AuditEntity> { public AuditEntityConfiguration() : base("Entity") { } }
