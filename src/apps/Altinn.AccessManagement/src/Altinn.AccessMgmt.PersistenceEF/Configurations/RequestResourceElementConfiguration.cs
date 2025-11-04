using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RequestResourceElementConfiguration : IEntityTypeConfiguration<RequestResourceElement>
{
    public void Configure(EntityTypeBuilder<RequestResourceElement> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RequestId).IsRequired();
        builder.Property(t => t.StatusId).IsRequired();
        builder.Property(t => t.ResourceId).IsRequired();
        builder.Property(t => t.ElementId).IsRequired();

        builder.PropertyWithReference(navKey: t => t.Request, foreignKey: t => t.RequestId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Status, foreignKey: t => t.StatusId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Resource, foreignKey: t => t.ResourceId, principalKey: t => t.Id, autoInclude: true);
        //// builder.PropertyWithReference(navKey: t => t.Element, foreignKey: t => t.ElementId, principalKey: t => t.Id, autoInclude: true);

        builder.HasIndex(["RequestId", "ElementId"]).IsUnique();
    }
}

public class AuditRequestResourceElementConfiguration : AuditConfiguration<AuditRequestResourceElement> { }
