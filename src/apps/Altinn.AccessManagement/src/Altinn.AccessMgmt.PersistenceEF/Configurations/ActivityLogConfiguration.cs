using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToDefaultTable();
        builder.HasPartitionByRange(nameof(ActivityLog.When));

        // The partition key must be part of the primary key on a partitioned table.
        builder.HasKey(p => new { p.When, p.Id });

        builder.Property(p => p.Id)
            .HasDefaultValueSql("dbo.uuid_generate_v7()")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Details).HasColumnType("jsonb");

        builder.HasIndex(p => new { p.FromId, p.When });
        builder.HasIndex(p => new { p.ToId, p.When });
        builder.HasIndex(p => p.ItemId);
        builder.HasIndex(p => p.ParentId);
    }
}
