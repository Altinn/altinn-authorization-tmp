using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RequestPackageConfiguration : IEntityTypeConfiguration<RequestPackage>
{
    public void Configure(EntityTypeBuilder<RequestPackage> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RequestId).IsRequired();
        builder.Property(t => t.StatusId).IsRequired();
        builder.Property(t => t.PackageId).IsRequired();

        builder.PropertyWithReference(navKey: t => t.Request, foreignKey: t => t.RequestId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Status, foreignKey: t => t.StatusId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Package, foreignKey: t => t.PackageId, principalKey: t => t.Id, autoInclude: true);

        builder.HasIndex(["RequestId", "PackageId"]).IsUnique();
    }
}

public class AuditRequestPackageConfiguration : AuditConfiguration<AuditRequestPackage> { }
