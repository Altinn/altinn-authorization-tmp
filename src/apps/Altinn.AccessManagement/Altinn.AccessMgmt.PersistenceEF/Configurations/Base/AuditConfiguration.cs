using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models.Audit.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Base;

public abstract class AuditConfiguration<T> : IEntityTypeConfiguration<T>, IAuditDbConfiguration
    where T : class, IAudit
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToDefaultAuditTable();
        builder.HasKey(["Id", "ChangeOperation"]);

        builder.Property(e => e.ValidFrom).HasColumnName("audit_validfrom");
        builder.Property(e => e.ValidTo).HasColumnName("audit_validto");
        builder.Property(e => e.ChangedBy).HasColumnName("audit_changedby");
        builder.Property(e => e.ChangedBySystem).HasColumnName("audit_changedbysystem");
        builder.Property(e => e.ChangeOperation).HasColumnName("audit_changeoperation");
        builder.Property(e => e.DeletedBy).HasColumnName("audit_deletedby");
        builder.Property(e => e.DeletedBySystem).HasColumnName("audit_deletedbysystem");
        builder.Property(e => e.DeleteOperation).HasColumnName("audit_deleteoperation");
    }
}
