using Altinn.AccessMgmt.PersistenceEF.Configurations.Base;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToDefaultTable();
        builder.EnableAudit();
        builder.Navigation(t => t.Status).AutoInclude();

        builder.HasKey(p => p.Id);

        builder.Property(t => t.RequestedById).IsRequired();
        builder.Property(t => t.StatusId).IsRequired();

        builder.Property(t => t.FromId).IsRequired();
        builder.Property(t => t.ToId).IsRequired();
        builder.Property(t => t.RoleId).IsRequired();
        builder.Property(t => t.ViaId);
        builder.Property(t => t.ViaRoleId);

        builder.PropertyWithReference(navKey: t => t.RequestedBy, foreignKey: t => t.RequestedById, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Status, foreignKey: t => t.StatusId, principalKey: t => t.Id, autoInclude: true);

        builder.PropertyWithReference(navKey: t => t.From, foreignKey: t => t.FromId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.To, foreignKey: t => t.ToId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Role, foreignKey: t => t.RoleId, principalKey: t => t.Id, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.Via, foreignKey: t => t.ViaId, principalKey: t => t.Id, required: false, autoInclude: true);
        builder.PropertyWithReference(navKey: t => t.ViaRole, foreignKey: t => t.ViaRoleId, principalKey: t => t.Id, required: false, autoInclude: true);

        builder.HasIndex(["FromId", "ToId", "RoleId", "RequestedById"]).IsUnique();
    }
}

public class AuditRequestConfiguration : AuditConfiguration<AuditRequest> { }
