using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource> {
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resource", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.Description).IsRequired();
        builder.Property(t => t.RefId);
        builder.Property(t => t.TypeId).IsRequired();
        builder.Property(t => t.ProviderId).IsRequired();

        //// builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedResourceConfiguration : IEntityTypeConfiguration<ExtendedResource> {
    public void Configure(EntityTypeBuilder<ExtendedResource> builder)
    {
        builder.ToTable("Resource", "dbo");
        builder.HasOne(p => p.Type).WithMany().HasForeignKey(p => p.TypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Provider).WithMany().HasForeignKey(p => p.ProviderId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditResourceConfiguration : AuditConfiguration<AuditResource> { public AuditResourceConfiguration() : base("Resource") { } }
