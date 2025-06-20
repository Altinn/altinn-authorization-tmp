using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider> {
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("Provider", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.RefId).IsRequired();
        builder.Property(t => t.Code).IsRequired();
        builder.Property(t => t.LogoUrl).IsRequired();
        builder.Property(t => t.TypeId).IsRequired();

        builder.HasIndex(t => t.Name).IsUnique();
    }
}

public class ExtendedProviderConfiguration : IEntityTypeConfiguration<ExtendedProvider> {
    public void Configure(EntityTypeBuilder<ExtendedProvider> builder)
    {
        builder.ToTable("Provider", "dbo");
        builder.HasOne(p => p.Type).WithMany().HasForeignKey(p => p.TypeId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditProviderConfiguration : AuditConfiguration<AuditProvider> { public AuditProviderConfiguration() : base("Provider") { } }
