using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class EntityLookupConfiguration : IEntityTypeConfiguration<EntityLookup> 
{
    public void Configure(EntityTypeBuilder<EntityLookup> builder)
    {
        builder.ToTable("EntityLookup", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.EntityId).IsRequired();
        builder.Property(t => t.Key).IsRequired();
        builder.Property(t => t.Value).IsRequired();

        builder.HasIndex(t => new { t.EntityId, t.Key }).IncludeProperties(p => new { p.Value, p.Id }).IsUnique();
    }
}

public class ExtendedEntityLookupConfiguration : IEntityTypeConfiguration<ExtEntityLookup> 
{
    public void Configure(EntityTypeBuilder<ExtEntityLookup> builder)
    {
        builder.ToTable("EntityLookup", "dbo");
        builder.HasOne(p => p.Entity).WithMany().HasForeignKey(p => p.EntityId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditEntityLookupConfiguration : AuditConfiguration<AuditEntityLookup> { public AuditEntityLookupConfiguration() : base("EntityLookup") { } }
