using Altinn.AccessMgmt.PersistenceEF.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public abstract class AuditConfiguration<T> : IEntityTypeConfiguration<T>
    where T : class, IAudit
{
    private readonly string _tableName;
    private readonly string _schema;

    protected AuditConfiguration(string tableName, string schema = "history")
    {
        _tableName = tableName;
        _schema = schema;
    }

    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(_tableName, _schema);
        builder.HasKey(e => EF.Property<Guid>(e, "Id")); // antar "Id" finnes fra base-entitet

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
