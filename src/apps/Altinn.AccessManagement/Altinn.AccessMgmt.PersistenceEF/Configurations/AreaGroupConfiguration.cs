using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class AreaGroupConfiguration : IEntityTypeConfiguration<AreaGroup>
{
    public void Configure(EntityTypeBuilder<AreaGroup> builder)
    {
        builder.ToTable("areagroup", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Urn).IsRequired();
        builder.Property(t => t.EntityTypeId).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedAreaGroupConfiguration : IEntityTypeConfiguration<ExtAreaGroup> 
{
    public void Configure(EntityTypeBuilder<ExtAreaGroup> builder)
    {
        builder.ToTable("areagroup", "dbo");
        builder.HasOne(p => p.EntityType).WithMany().HasForeignKey(p => p.EntityTypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditAreaGroupConfiguration : AuditConfiguration<AuditAreaGroup> { public AuditAreaGroupConfiguration() : base("areagroup") { } }
