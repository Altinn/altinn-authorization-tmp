using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ResourceTypeConfiguration : IEntityTypeConfiguration<ResourceType> {
    public void Configure(EntityTypeBuilder<ResourceType> builder)
    {
        builder.ToTable("ResourceType", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class AuditResourceTypeConfiguration : AuditConfiguration<AuditResourceType> { public AuditResourceTypeConfiguration() : base("ResourceType") { } }
