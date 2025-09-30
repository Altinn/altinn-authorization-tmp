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
        //builder.PropertyWithReference(navKey: t => t.AltinnApp, foreignKey: t => t.AltinnAppId, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);

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

        //builder.PropertyWithReference(navKey: t => t.PerformedBy, foreignKey: t => t.PerformedByUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict);
        builder.PropertyWithReference(navKey: t => t.From, foreignKey: t => t.FromUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict, required: false);
        builder.PropertyWithReference(navKey: t => t.To, foreignKey: t => t.ToUuid, principalKey: t => t.Id, deleteBehavior: DeleteBehavior.Restrict, required: false);

        builder.Property(x => x.DelegationChangeType).HasColumnName("delegationchangetype").IsRequired(); //.HasColumnType<DelegationChangeType>("delegation.delegationchangetype").IsRequired();
        builder.Property(x => x.Created).HasColumnName("created");

        builder.Property(x => x.OfferedByPartyId).HasColumnName("offeredbypartyid");

        builder.Property(x => x.CoveredByPartyId).HasColumnName("coveredbypartyid");
        builder.Property(x => x.CoveredByUserId).HasColumnName("coveredbyuserid");

        builder.Property(x => x.PerformedByUserId).HasColumnName("performedbyuserid");
        //builder.Property(x => x.PerformedByPartyId).HasColumnName("performedbypartyid");
        builder.Property(x => x.PerformedByUuid).HasColumnName("performedbyuuid");
        builder.Property(x => x.PerformedByUuidType).HasColumnName("performedbytype");//.HasColumnType<UuidType>("uuidtype");

        builder.Property(x => x.BlobStoragePolicyPath).HasColumnName("blobstoragepolicypath");
        builder.Property(x => x.BlobStorageVersionId).HasColumnName("blobstorageversionid");

        builder.Property(x => x.FromUuid).HasColumnName("fromuuid");
        builder.Property(x => x.FromUuidType).HasColumnName("fromtype");//.HasColumnType<UuidType>("uuidtype");

        builder.Property(x => x.ToUuid).HasColumnName("touuid");
        builder.Property(x => x.ToUuidType).HasColumnName("totype");//.HasColumnType<UuidType>("uuidtype");

        return builder;
    }
}



/*
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 51423446, null, 20012655, 20012650, 'ttd/apps-test-tba/51423446/u20012655/delegationpolicy.xml', '2025-09-30T12:43:28.1441196Z', '2025-09-30 12:43:28.179997 +00:00', 'eac6fcf7-51ca-42a5-80d5-1670eacfab0a', 'urn:altinn:organization:uuid', '7bdd6a6b-b771-4674-b567-1fd1a5db4aee', 'urn:altinn:person:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 50717658, 51658512, null, 20012043, 'ttd/apps-test-tba/50717658/p51658512/delegationpolicy.xml', '2025-09-30T12:43:26.9772501Z', '2025-09-30 12:43:27.001090 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 50717658, 51658512, null, 20012043, 'ttd/apps-test-tba/50717658/p51658512/delegationpolicy.xml', '2025-09-30T12:43:26.6705969Z', '2025-09-30 12:43:26.707244 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 50717658, 51658512, null, 20012044, 'ttd/apps-test-tba/50717658/p51658512/delegationpolicy.xml', '2025-09-30T12:43:25.9557393Z', '2025-09-30 12:43:25.983758 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 50717658, 51658512, null, 20012043, 'ttd/apps-test-tba/50717658/p51658512/delegationpolicy.xml', '2025-09-30T12:43:25.4390108Z', '2025-09-30 12:43:25.463687 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 51350052, 51658512, null, 20012043, 'ttd/apps-test-tba/51350052/p51658512/delegationpolicy.xml', '2025-09-30T12:43:24.8884312Z', '2025-09-30 12:43:24.916239 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 51350052, 51658512, null, 20012043, 'ttd/apps-test-tba/51350052/p51658512/delegationpolicy.xml', '2025-09-30T12:43:24.5937266Z', '2025-09-30 12:43:24.623301 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 51350052, 51658512, null, 20012044, 'ttd/apps-test-tba/51350052/p51658512/delegationpolicy.xml', '2025-09-30T12:43:23.8947994Z', '2025-09-30 12:43:23.924100 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 51350052, 51658512, null, 20012043, 'ttd/apps-test-tba/51350052/p51658512/delegationpolicy.xml', '2025-09-30T12:43:23.3362544Z', '2025-09-30 12:43:23.359996 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', '8ea2c3f4-ade4-4b33-be62-080d62566da3', 'urn:altinn:organization:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 51350052, null, 20012044, 20012043, 'ttd/apps-test-tba/51350052/u20012044/delegationpolicy.xml', '2025-09-30T12:43:22.5188476Z', '2025-09-30 12:43:22.548437 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 51350052, null, 20012044, 20012043, 'ttd/apps-test-tba/51350052/u20012044/delegationpolicy.xml', '2025-09-30T12:43:22.1912878Z', '2025-09-30 12:43:22.211540 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 51350052, null, 20012044, 20012044, 'ttd/apps-test-tba/51350052/u20012044/delegationpolicy.xml', '2025-09-30T12:43:21.3957845Z', '2025-09-30 12:43:21.420870 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 51350052, null, 20012044, 20012043, 'ttd/apps-test-tba/51350052/u20012044/delegationpolicy.xml', '2025-09-30T12:43:20.7486295Z', '2025-09-30 12:43:20.792161 +00:00', 'f200a9cb-31ce-4ed6-aad3-ed08b3cbbeee', 'urn:altinn:organization:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 50717658, null, 20012044, 20012043, 'ttd/apps-test-tba/50717658/u20012044/delegationpolicy.xml', '2025-09-30T12:43:19.9710476Z', '2025-09-30 12:43:19.989310 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 50717658, null, 20012044, 20012043, 'ttd/apps-test-tba/50717658/u20012044/delegationpolicy.xml', '2025-09-30T12:43:19.5479076Z', '2025-09-30 12:43:19.575669 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 50717658, null, 20012044, 20012044, 'ttd/apps-test-tba/50717658/u20012044/delegationpolicy.xml', '2025-09-30T12:43:18.5254015Z', '2025-09-30 12:43:18.548733 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test-tba', 50717658, null, 20012044, 20012043, 'ttd/apps-test-tba/50717658/u20012044/delegationpolicy.xml', '2025-09-30T12:43:17.7388596Z', '2025-09-30 12:43:17.756300 +00:00', '24425b64-9d4d-4bf6-8ba1-a33e58882a4e', 'urn:altinn:person:uuid', 'e5c0f84a-3986-49f4-a6a9-e935e1f03bc0', 'urn:altinn:person:uuid', null, 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test-tba', 51423446, null, 20012655, 20012655, 'ttd/apps-test-tba/51423446/u20012655/delegationpolicy.xml', '2025-09-30T12:43:16.7780841Z', '2025-09-30 12:43:16.805945 +00:00', 'eac6fcf7-51ca-42a5-80d5-1670eacfab0a', 'urn:altinn:organization:uuid', '7bdd6a6b-b771-4674-b567-1fd1a5db4aee', 'urn:altinn:person:uuid', '00000000-0000-0000-0000-000000000000', 'urn:altinn:party:uuid');
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test', 50066583, 50066621, null, 20003934, 'ttd/apps-test/50066583/p50066621/delegationpolicy.xml', '2025-09-30T12:42:49.3436720Z', '2025-09-30 12:42:49.369380 +00:00', '00000000-0000-0000-0000-000000000000', null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test', 50066583, 50066621, null, 20003934, 'ttd/apps-test/50066583/p50066621/delegationpolicy.xml', '2025-09-30T12:42:45.7703777Z', '2025-09-30 12:42:45.806051 +00:00', null, null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test', 50066757, 50066621, null, 20002630, 'ttd/apps-test/50066757/p50066621/delegationpolicy.xml', '2025-09-30T12:42:39.4989424Z', '2025-09-30 12:42:39.525134 +00:00', '00000000-0000-0000-0000-000000000000', null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test', 50066757, 50066621, null, 20002630, 'ttd/apps-test/50066757/p50066621/delegationpolicy.xml', '2025-09-30T12:42:36.2014354Z', '2025-09-30 12:42:36.258257 +00:00', null, null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test', 50066757, 50066621, null, 20002630, 'ttd/apps-test/50066757/p50066621/delegationpolicy.xml', '2025-09-30T12:42:30.0186093Z', '2025-09-30 12:42:30.043567 +00:00', '00000000-0000-0000-0000-000000000000', null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('grant', 'ttd/apps-test', 50066757, 50066621, null, 20002630, 'ttd/apps-test/50066757/p50066621/delegationpolicy.xml', '2025-09-30T12:42:26.7708829Z', '2025-09-30 12:42:26.798394 +00:00', null, null, null, null, null, null);
INSERT INTO delegation.delegationchanges (delegationchangetype, altinnappid, offeredbypartyid, coveredbypartyid, coveredbyuserid, performedbyuserid, blobstoragepolicypath, blobstorageversionid, created, fromuuid, fromtype, touuid, totype, performedbyuuid, performedbytype) VALUES ('revoke_last', 'ttd/apps-test', 50066757, null, 20004677, 20002630, 'ttd/apps-test/50066757/u20004677/delegationpolicy.xml', '2025-09-30T12:42:20.6029899Z', '2025-09-30 12:42:20.627736 +00:00', '00000000-0000-0000-0000-000000000000', null, null, null, null, null);
 
*/
