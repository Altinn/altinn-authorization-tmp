using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ProviderTypeConfiguration : IEntityTypeConfiguration<ProviderType> {
    public void Configure(EntityTypeBuilder<ProviderType> builder)
    {
        builder.ToTable("ProviderType", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedProviderTypeConfiguration : IEntityTypeConfiguration<ExtendedProviderType>
{
    public void Configure(EntityTypeBuilder<ExtendedProviderType> builder) { }
}

public class AuditProviderTypeConfiguration : AuditConfiguration<AuditProviderType> { public AuditProviderTypeConfiguration() : base("ProviderType") { } }
