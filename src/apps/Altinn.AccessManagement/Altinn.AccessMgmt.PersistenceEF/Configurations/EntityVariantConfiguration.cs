using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class EntityVariantConfiguration : IEntityTypeConfiguration<EntityVariant> {
    public void Configure(EntityTypeBuilder<EntityVariant> builder)
    {
        builder.ToTable("EntityVariant", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.TypeId).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedEntityVariantConfiguration : IEntityTypeConfiguration<ExtendedEntityVariant>
{
    public void Configure(EntityTypeBuilder<ExtendedEntityVariant> builder)
    {
        builder.ToTable("EntityVariant", "dbo");
        builder.HasOne(p => p.Type).WithMany().HasForeignKey(p => p.TypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditEntityVariantConfiguration : AuditConfiguration<AuditEntityVariant> { public AuditEntityVariantConfiguration() : base("EntityVariant") { } }
