using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class AreaConfiguration : IEntityTypeConfiguration<Area> 
{
    public void Configure(EntityTypeBuilder<Area> builder)
    {
        builder.ToTable("area", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.Urn).IsRequired();
        builder.Property(t => t.IconUrl);
        builder.Property(t => t.GroupId).IsRequired();
        
        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedAreaConfiguration : IEntityTypeConfiguration<ExtArea> 
{
    public void Configure(EntityTypeBuilder<ExtArea> builder)
    {
        builder.ToTable("area", "dbo");
        builder.HasOne(p => p.Group).WithMany().HasForeignKey(p => p.GroupId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditAreaConfiguration : AuditConfiguration<AuditArea> { public AuditAreaConfiguration() : base("area") { } }
