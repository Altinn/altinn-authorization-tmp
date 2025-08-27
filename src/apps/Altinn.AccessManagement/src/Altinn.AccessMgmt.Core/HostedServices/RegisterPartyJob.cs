using System.Diagnostics;
using System.Diagnostics.Metrics;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.HostedServices;

public class RegisterPartyJob(IAltinnRegister Register, IIngestService IngestService) : JobBase
{
    private const string Person = "Person";

    private const string Organization = "Organisasjon";

    private const string SelfIdentifiedUser = "SI";

    private const int BulkSize = 10_000;

    private Task<JobResult?> FlushTask { get; set; } = Task.FromResult<JobResult?>(null);

    private static readonly Counter<int> EntityMergedRows = TelemetryConfig.Meter
        .CreateCounter<int>("Entity.MergedRows", "rows", "Records how many rows were upserted into Entity table.");

    private static readonly Counter<int> EntityLookupMergedRows = TelemetryConfig.Meter
        .CreateCounter<int>("EntityLookup.MergedRows", "rows", "Records how many rows were upserted into EntityLookup table.");

    private static readonly Counter<int> EntityTypes = TelemetryConfig.Meter
        .CreateCounter<int>("Entity.Types", "types", "Records all types received from register.");

    private static readonly Counter<int> EntityUnkownTypes = TelemetryConfig.Meter
        .CreateCounter<int>("Entity.UnkownTypes", "types", "Records all unkown / unmapped types received from register.");

    private static readonly IReadOnlyList<string> EntityMergeMatchFilter = new List<string>() { "id" }.AsReadOnly();

    private static readonly IReadOnlyList<string> EntityLookupMergeMatchFilter = new List<string>() { "entityid", "key" }.AsReadOnly();

    public override async Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        var featureManager = context.ServiceProvider.GetRequiredService<IFeatureManager>();
        return !await featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesRegisterSync);
    }

    public override async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        using var lease = await context.Lease.TryAquireNonBlocking<LeaseContent>("access_management_register_party_sync", cancellationToken);
        if (!lease.HasLease)
        {
            return JobResult.CouldNotRun("Failed to aquire lease initially");
        }

        try
        {
            var dbContext = context.ServiceProvider.GetRequiredService<AppDbContext>();
            var state = await State.Create(dbContext, lease, cancellationToken);
            var currentPage = lease.Data?.NextPage;
            await foreach (var page in await Register.StreamParties(AltinnRegisterClient.AvailableFields, lease.Data.NextPage, cancellationToken))
            {
                if (page.IsProblem)
                {
                    return JobResult.Failure(page.ProblemDetails);
                }

                var result = JobHasLease(lease);
                if (result is { })
                {
                    return result;
                }

                result = await ProcessPage(state, context, page.Content.Data, currentPage, cancellationToken);
                if (result is { })
                {
                    return result;
                }

                if (page?.Content?.Links?.Next is null)
                {
                    break;
                }

                currentPage = page.Content.Links.Next;
            }

            if (state.Entities.Count > 0)
            {
                await Flush(state, context, currentPage, cancellationToken);
            }
        }
        catch (OperationCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            return JobResult.Cancelled("Cancellation was requested.", ex);
        }
        catch (Exception ex)
        {
            return JobResult.Failure(ex);
        }

        return JobResult.Success("Successfully processed all parties.");
    }

    private async Task<JobResult?> ProcessPage(State state, JobContext context, IEnumerable<PartyModel> parties, string currentPage, CancellationToken cancellationToken)
    {
        foreach (var party in parties)
        {
            if (MustFlush(state, party))
            {
                var result = await Flush(state, context, currentPage, cancellationToken);
                if (result is { })
                {
                    return result;
                }
            }

            AddEntity(state, party);
            AddEntityLookup(state, party);
        }

        return null;
    }

    private AuditValues NewAudit()
    {
        var operationId = Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString();
        return new AuditValues(
            Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B"),
            Guid.Parse("EFEC83FC-DEBA-4F09-8073-B4DD19D0B16B"),
            operationId
        );
    }

    private bool MustFlush(State state, PartyModel party)
    {
        var partyUuid = Guid.Parse(party.PartyUuid);
        if (state.Entities.Any(entity => entity.Id == partyUuid) || state.Entities.Count >= BulkSize)
        {
            return true;
        }

        return false;
    }

    private async Task<JobResult?> Flush(State state, JobContext context, string currentPage, CancellationToken cancellationToken)
    {
        var result = await FlushTask;
        if (result is { })
        {
            return result;
        }

        var (entities, lookups) = state.ReadyForFlush();
        FlushTask = IngestEntites();
        return null;

        async Task<JobResult?> IngestEntites()
        {
            try
            {
                var audit = NewAudit();
                var ingestedEntities = await IngestService.IngestAndMergeData(entities, audit, EntityMergeMatchFilter, cancellationToken);
                EntityMergedRows.Add(ingestedEntities);
                var ingestedLookups = await IngestService.IngestAndMergeData(lookups, audit, EntityLookupMergeMatchFilter, cancellationToken);
                EntityLookupMergedRows.Add(ingestedLookups);

                await context.Lease.UpsertLeastAndRefresh(state.Lease, content => content.NextPage = currentPage, cancellationToken);
            }
            catch (Exception ex)
            {
                return JobResult.Failure(ex);
            }

            return null;
        }
    }

    private static void AddEntityLookup(State state, PartyModel party)
    {
        static void SetKeyValue(EntityLookup lookup, string key, string value, bool isProtected = false)
        {
            lookup.Key = key;
            lookup.Value = value;
            lookup.IsProtected = isProtected;
        }

        List<Action<EntityLookup>> configureEntity = party.PartyType switch
        {
            string type when type.Equals("person", StringComparison.OrdinalIgnoreCase) =>
            [
                lookup => SetKeyValue(lookup, "DateOfBirth", party.DateOfBirth),
                lookup => SetKeyValue(lookup, "PartyId", party.PartyId.ToString()),
                lookup => SetKeyValue(lookup, "PersonIdentifier", party.PersonIdentifier, true),
                lookup => SetKeyValue(lookup, "UserId", party.User.UserId.ToString()),
            ],
            string type when type.Equals("organization", StringComparison.OrdinalIgnoreCase) =>
            [
                lookup => SetKeyValue(lookup, "PartyId", party.PartyId.ToString()),
                lookup => SetKeyValue(lookup, "OrganizationIdentifier", party.OrganizationIdentifier),
            ],
            string type when type.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase) =>
            [
                lookup => SetKeyValue(lookup, "PartyId", party.PartyId.ToString()),
            ],
            _ => [],
        };

        foreach (var configure in configureEntity)
        {
            var entity = new EntityLookup
            {
                Id = Guid.Parse(party.PartyUuid),
                IsProtected = false,
            };

            configure(entity);
            state.Lookups.Add(entity);
        }
    }

    private static void AddEntity(State state, PartyModel party)
    {
        static void SetIds(Entity entity, Guid typeId, Guid variantId, string refId)
        {
            entity.TypeId = typeId;
            entity.VariantId = variantId;
            entity.RefId = refId;
        }

        var entity = new Entity
        {
            Id = Guid.Parse(party.PartyUuid),
            Name = party.DisplayName,
        };

        Action<Entity> configureEntity = party.PartyType switch
        {
            string type when type.Equals("person", StringComparison.OrdinalIgnoreCase)
                => entity => SetIds(entity, state.PersonEntityType.Id, state.PersonEntityVariant.Id, party.PersonIdentifier),
            string type when type.Equals("organization", StringComparison.OrdinalIgnoreCase)
                => entity => SetIds(entity, state.OrganizationEntityType.Id, state.OrganizationEntityVariant.Id, party.OrganizationIdentifier),
            string type when type.Equals("self-identified-user", StringComparison.OrdinalIgnoreCase)
                => entity => SetIds(entity, state.PersonEntityType.Id, state.SelfIdentifiedUserEntityVariant.Id, party.VersionId.ToString()),
            _ => _ => { EntityUnkownTypes.Add(1, [new("type", party.PartyType)]); }
            ,
        };

        EntityTypes.Add(1, [new("type", party.PartyType)]);

        if (configureEntity is { })
        {
            configureEntity(entity);
            state.Entities.Add(entity);
        }
    }

    private class State
    {
        internal (List<Entity> Entities, List<EntityLookup> Lookups) ReadyForFlush()
        {
            var entities = Entities;
            var lookups = Lookups;

            Entities = [];
            Lookups = [];

            return (entities, lookups);
        }

        internal static async Task<State> Create(AppDbContext dbContext, LeaseResult<LeaseContent> lease, CancellationToken cancellationToken)
        {
            var types = await dbContext.EntityTypes.ToListAsync(cancellationToken);
            var variants = await dbContext.EntityVariants.ToListAsync(cancellationToken);
            return new State()
            {
                Lease = lease,
                DbContext = dbContext,
                PersonEntityType = types.First(t => t.Name == Person),
                OrganizationEntityType = types.First(t => t.Name == Organization),
                PersonEntityVariant = variants.First(v => v.TypeId == types.First(t => t.Name == Person).Id && v.Name == Person),
                OrganizationEntityVariant = variants.First(v => v.TypeId == types.First(t => t.Name == Organization).Id && v.Name == Organization),
                SelfIdentifiedUserEntityVariant = variants.First(v => v.TypeId == types.First(t => t.Name == Person).Id && v.Name == SelfIdentifiedUser),
            };
        }

        public LeaseResult<LeaseContent> Lease { get; set; }

        public AppDbContext DbContext { get; set; }

        public List<Entity> Entities { get; set; } = [];

        public List<EntityLookup> Lookups { get; set; } = [];

        public EntityVariant PersonEntityVariant { get; init; }

        public EntityVariant OrganizationEntityVariant { get; init; }

        public EntityVariant SelfIdentifiedUserEntityVariant { get; init; }

        public EntityType PersonEntityType { get; init; }

        public EntityType OrganizationEntityType { get; init; }
    }

    internal class LeaseContent
    {
        public string NextPage { get; set; }
    }
}
