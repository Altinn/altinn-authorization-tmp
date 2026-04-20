using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class DelegationPackageConfiguration : IEntityTypeConfiguration<DelegationPackage>
{
    public void Configure(EntityTypeBuilder<DelegationPackage> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();

        builder.HasKey(p => p.Id);

        builder.PropertyWithReference(navKey: t => t.Package, foreignKey: t => t.PackageId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

        builder.HasOne(t => t.Delegation)
            .WithMany(t => t.DelegationPackages)
            .HasForeignKey(t => t.DelegationId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(t => t.DelegationId);
        builder.HasIndex(t => new { t.DelegationId, t.PackageId }).IsUnique();
    }
}

public class AuditDelegationPackageConfiguration : AuditConfiguration<AuditDelegationPackage> { }
