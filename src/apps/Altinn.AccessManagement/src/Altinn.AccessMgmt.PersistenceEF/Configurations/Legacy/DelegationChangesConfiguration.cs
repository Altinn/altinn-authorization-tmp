using Altinn.AccessMgmt.PersistenceEF.Models.Legacy;
using Altinn.AccessMgmt.PersistenceEF.Models.Legacy.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Altinn.AccessMgmt.PersistenceEF.Configurations.Legacy;

public class DelegationChangesConfiguration : IEntityTypeConfiguration<DelegationChanges>
{
    public void Configure(EntityTypeBuilder<DelegationChanges> builder)
    {
        builder.ToTable<DelegationChanges>(nameof(DelegationChanges), "delegation");

        builder.HasKey(p => p.DelegationChangeId);

        builder.Property(x => x.DelegationChangeId).HasColumnName("delegationchangeid").IsRequired();
        builder.Property(x => x.DelegationChangeType).HasColumnName("delegationchangetype").HasColumnType<DelegationChangeType>("delegationchangetype").IsRequired();
        builder.Property(x => x.AltinnAppId).HasColumnName("altinnappid");
        builder.Property(x => x.OfferedByPartyId).HasColumnName("offeredbypartyid");
        builder.Property(x => x.CoveredByPartyId).HasColumnName("coveredbypartyid");
        builder.Property(x => x.CoveredByUserId).HasColumnName("coveredbyuserid");
        builder.Property(x => x.PerformedByUserId).HasColumnName("performedbyuserid");
        builder.Property(x => x.BlobStoragePolicyPath).HasColumnName("blobstoragepolicypath");
        builder.Property(x => x.BlobStorageVersionId).HasColumnName("blobstorageversionid");
        builder.Property(x => x.Created).HasColumnName("created");
        builder.Property(x => x.FromUuid).HasColumnName("fromuuid");
        builder.Property(x => x.FromUuidType).HasColumnName("fromtype").HasColumnType<UuidType>("uuidtype");
        builder.Property(x => x.ToUuid).HasColumnName("touuid");
        builder.Property(x => x.ToUuidType).HasColumnName("totype").HasColumnType<UuidType>("uuidtype");
        builder.Property(x => x.PerformedByUuid).HasColumnName("performedbyuuid");
        builder.Property(x => x.PerformedByUuidType).HasColumnName("performedbytype").HasColumnType<UuidType>("uuidtype");
    }
}
