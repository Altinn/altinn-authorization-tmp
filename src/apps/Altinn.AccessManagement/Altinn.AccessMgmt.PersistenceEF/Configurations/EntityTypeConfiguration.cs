using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class EntityTypeConfiguration : IEntityTypeConfiguration<EntityType>
{
    public void Configure(EntityTypeBuilder<EntityType> builder)
    {
        builder.ToTable("entitytype", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.ProviderId).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedEntityTypeConfiguration : IEntityTypeConfiguration<ExtendedEntityType>
{
    public void Configure(EntityTypeBuilder<ExtendedEntityType> builder)
    {
        builder.ToTable("entitytype", "dbo");
        builder.HasOne(p => p.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditEntityTypeConfiguration : AuditConfiguration<AuditEntityType> { public AuditEntityTypeConfiguration() : base("EntityType") { } }
