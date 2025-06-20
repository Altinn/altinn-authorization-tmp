using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class DelegationConfiguration : IEntityTypeConfiguration<Delegation> {
    public void Configure(EntityTypeBuilder<Delegation> builder)
    {
        builder.ToTable("Delegation", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.FromId).IsRequired();
        builder.Property(t => t.ToId).IsRequired();
        builder.Property(t => t.FacilitatorId).IsRequired();

        builder.HasIndex(t => new { t.FromId, t.ToId, t.FacilitatorId }).IsUnique();
    }
}

public class ExtendedDelegationConfiguration : IEntityTypeConfiguration<ExtendedDelegation> {
    public void Configure(EntityTypeBuilder<ExtendedDelegation> builder)
    {
        builder.ToTable("Delegation", "dbo");
        builder.HasOne(p => p.From).WithMany().HasForeignKey(p => p.FromId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.To).WithMany().HasForeignKey(p => p.ToId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Facilitator).WithMany().HasForeignKey(p => p.FacilitatorId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditDelegationConfiguration : AuditConfiguration<AuditDelegation> { public AuditDelegationConfiguration() : base("Delegation") { } }
