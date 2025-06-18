using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class DelegationPackageConfiguration : IEntityTypeConfiguration<DelegationPackage>
{
    public void Configure(EntityTypeBuilder<DelegationPackage> builder)
    {
        builder.ToTable("DelegationPackage", "dbo");

        builder.HasKey(p => p.Id);

        builder.Property(t => t.DelegationId).IsRequired();
        builder.Property(t => t.PackageId).IsRequired();

        builder.HasIndex(t => new { t.DelegationId, t.PackageId }).IsUnique();
    }
}

public class ExtendedDelegationPackageConfiguration : IEntityTypeConfiguration<ExtDelegationPackage> {
    public void Configure(EntityTypeBuilder<ExtDelegationPackage> builder)
    {
        builder.ToTable("DelegationPackage", "dbo");
        builder.HasOne(p => p.Delegation).WithMany().HasForeignKey(p => p.DelegationId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Package).WithMany().HasForeignKey(p => p.PackageId).HasPrincipalKey(c => c.Id).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AuditDelegationPackageConfiguration : AuditConfiguration<AuditDelegationPackage> { public AuditDelegationPackageConfiguration() : base("DelegationPackage") { } }
