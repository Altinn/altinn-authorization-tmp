using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class DelegationResourceConfiguration : IEntityTypeConfiguration<DelegationResource> {
    public void Configure(EntityTypeBuilder<DelegationResource> builder)
    {
        builder.ToTable("DelegationResource", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.DelegationId).IsRequired();
        builder.Property(t => t.ResourceId).IsRequired();

        builder.HasIndex(t => new { t.DelegationId, t.ResourceId }).IsUnique();
    }
}

public class ExtendedDelegationResourceConfiguration : IEntityTypeConfiguration<ExtDelegationResource>
{
    public void Configure(EntityTypeBuilder<ExtDelegationResource> builder)
    {
        builder.ToTable("DelegationResource", "dbo");
        builder.HasOne(p => p.Delegation).WithMany().HasForeignKey(p => p.DelegationId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Resource).WithMany().HasForeignKey(p => p.ResourceId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditDelegationResourceConfiguration : AuditConfiguration<AuditDelegationResource> { public AuditDelegationResourceConfiguration() : base("DelegationResource") { } }
