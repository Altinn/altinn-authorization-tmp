using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class PackageHistoryConfiguration : IEntityTypeConfiguration<AuditPackage>
{
    public void Configure(EntityTypeBuilder<AuditPackage> builder)
    {
        builder.ToTable("package", "history");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.Description).IsRequired();

        builder.Property(p => p.ValidFrom).HasColumnName("audit_validfrom");
        builder.Property(p => p.ValidTo).HasColumnName("audit_validto");

        builder.Property(p => p.ChangedBy).HasColumnName("audit_changedby");
        builder.Property(p => p.ChangedBySystem).HasColumnName("audit_changedbysystem");
        builder.Property(p => p.ChangeOperation).HasColumnName("audit_changeoperation");

        builder.Property(p => p.DeletedBy).HasColumnName("audit_deletedby");
        builder.Property(p => p.DeletedBySystem).HasColumnName("audit_deletedbysystem");
        builder.Property(p => p.DeleteOperation).HasColumnName("audit_deleteoperation");
    }
}
