using Altinn.AccessMgmt.PersistenceEF.Extensions;
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
        builder.PropertyWithReference(navKey: t => t.AltinnApp, foreignKey: t => t.AltinnAppId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

        builder.ConfigureLeagacyCommonChanges();
    }
}

public class ResourceRegistryDelegationChangesConfiguration : IEntityTypeConfiguration<ResourceRegistryDelegationChanges>
{
    public void Configure(EntityTypeBuilder<ResourceRegistryDelegationChanges> builder)
    {
        builder.ToTable<ResourceRegistryDelegationChanges>(nameof(ResourceRegistryDelegationChanges), "delegation");

        builder.HasKey(p => p.ResourceRegistryDelegationChangeId);

        builder.Property(x => x.ResourceRegistryDelegationChangeId).HasColumnName("resourceregistrydelegationchangeid").IsRequired();
        builder.HasOne(t => t.Resource).WithMany().HasForeignKey("resourceid_fk").HasPrincipalKey("Id").OnDelete(deleteBehavior: DeleteBehavior.Restrict);

        builder.ConfigureLeagacyCommonChanges();
    }
}

public class InstanceDelegationChangesConfiguration : IEntityTypeConfiguration<InstanceDelegationChanges>
{
    public void Configure(EntityTypeBuilder<InstanceDelegationChanges> builder)
    {
        builder.ToTable<InstanceDelegationChanges>(nameof(InstanceDelegationChanges), "delegation");

        builder.HasKey(p => p.DelegationChangeId);

        builder.Property(x => x.DelegationChangeId).HasColumnName("resourceregistrydelegationchangeid").IsRequired();
        builder.Property(x => x.InstanceId).HasColumnName("instanceid").IsRequired();
        builder.PropertyWithReference(navKey: t => t.Resource, foreignKey: t => t.ResourceId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

        builder.ConfigureLeagacyCommonChanges();
    }
}

public static class LeagacyBuilderExtensions
{
    public static EntityTypeBuilder<TEntity> ConfigureLeagacyCommonChanges<TEntity>(this EntityTypeBuilder<TEntity> builder) 
        where TEntity : class, ICommonDelegationChanges
    {
        //// builder.PropertyWithReference(navKey: t => t.OfferedBy, foreignKey: t => t.OfferedByPartyId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);
        //// builder.PropertyWithReference(navKey: t => t.CoveredBy, foreignKey: t => t.CoveredByPartyId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

        builder.PropertyWithReference(navKey: t => t.PerformedBy, foreignKey: t => t.PerformedByUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);
        builder.PropertyWithReference(navKey: t => t.From, foreignKey: t => t.FromUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);
        builder.PropertyWithReference(navKey: t => t.To, foreignKey: t => t.ToUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

        builder.Property(x => x.DelegationChangeType).HasColumnName("delegationchangetype").HasColumnType<DelegationChangeType>("delegationchangetype").IsRequired();
        builder.Property(x => x.Created).HasColumnName("created");

        builder.Property(x => x.OfferedByPartyId).HasColumnName("offeredbypartyid");

        builder.Property(x => x.CoveredByPartyId).HasColumnName("coveredbypartyid");
        builder.Property(x => x.CoveredByUserId).HasColumnName("coveredbyuserid");

        builder.Property(x => x.PerformedByUserId).HasColumnName("performedbyuserid");
        builder.Property(x => x.PerformedByPartyId).HasColumnName("performedbypartyid");
        builder.Property(x => x.PerformedByUuid).HasColumnName("performedbyuuid");
        builder.Property(x => x.PerformedByUuidType).HasColumnName("performedbytype").HasColumnType<UuidType>("uuidtype");

        builder.Property(x => x.BlobStoragePolicyPath).HasColumnName("blobstoragepolicypath");
        builder.Property(x => x.BlobStorageVersionId).HasColumnName("blobstorageversionid");

        builder.Property(x => x.FromUuid).HasColumnName("fromuuid");
        builder.Property(x => x.FromUuidType).HasColumnName("fromtype").HasColumnType<UuidType>("uuidtype");

        builder.Property(x => x.ToUuid).HasColumnName("touuid");
        builder.Property(x => x.ToUuidType).HasColumnName("totype").HasColumnType<UuidType>("uuidtype");

        return builder;
    }
}
