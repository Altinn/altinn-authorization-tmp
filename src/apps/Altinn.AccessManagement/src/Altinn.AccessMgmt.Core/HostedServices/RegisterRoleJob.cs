using System.Diagnostics;
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

public class RegisterRoleJob(
    IAltinnRegister Register,
    IIngestService IngestService
    ) : JobBase
{
    private const int BulkSize = 10_000;

    private Task<JobResult?> FlushTask { get; set; } = Task.FromResult<JobResult?>(null);

    private static readonly IReadOnlyList<string> AssignmentMergeMatchFilter = new List<string>() { "fromid", "toid", "roleid", }.AsReadOnly();

    private static readonly IReadOnlyList<string> RoleMergeMatchFilter = new List<string>() { "urn" }.AsReadOnly();

    public override async Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        var featureManager = context.ServiceProvider.GetRequiredService<IFeatureManager>();
        return !await featureManager.IsEnabledAsync(AccessMgmtFeatureFlags.HostedServicesRegisterSync);
    }

    public override async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        using var lease = await context.Lease.TryAquireNonBlocking<LeaseContent>("access_management_register_role_sync", cancellationToken);
        if (!lease.HasLease)
        {
            return JobResult.CouldNotRun("Failed to aquire lease initially");
        }

        try
        {
            var dbContext = context.ServiceProvider.GetRequiredService<AppDbContext>();
            var state = await State.Create(dbContext, lease, cancellationToken);
            var currentPage = lease.Data?.NextPage;
            await foreach (var page in await Register.StreamRoles([], lease.Data.NextPage, cancellationToken))
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
                await context.Lease.UpsertLeastAndRefresh(lease, content => content.NextPage = currentPage, cancellationToken);
            }

            if (state.NewAssignments.Count > 0)
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

    private async Task<JobResult?> ProcessPage(State state, JobContext context, IEnumerable<RoleModel> roles, string currentPage, CancellationToken cancellationToken)
    {
        foreach (var role in roles)
        {
            if (MustFlush(state))
            {
                var result = await Flush(state, context, currentPage, cancellationToken);
                if (result is { })
                {
                    return result;
                }
            }

            AddRole(state, role);
            AddAssignment(state, role);
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

    private bool MustFlush(State state)
    {
        if (state.NewAssignments.Count + state.NewRoles.Count >= BulkSize)
        {
            return true;
        }

        return false;
    }

    private JobResult? AddAssignment(State state, RoleModel model)
    {
        var role = state.AllRoles.FirstOrDefault(r => r.Code == model.RoleIdentifier);
        if (role is null)
        {
            return JobResult.Failure($"Could not find role with code {model.RoleIdentifier} for assignment from {model.FromParty} to {model.ToParty}");
        }

        state.NewAssignments.Add(new()
        {
            Id = Guid.CreateVersion7(),
            FromId = Guid.Parse(model.FromParty),
            ToId = Guid.Parse(model.ToParty),
            RoleId = role.Id,
        });

        return null;
    }

    private void AddRole(State state, RoleModel role)
    {
        if (state.AllRoles.Any(r => r.Code == role.RoleIdentifier))
        {
            return;
        }

        state.NewRoles.Add(new()
        {
            Id = Guid.CreateVersion7(),
            Name = role.RoleIdentifier,
            Description = role.RoleIdentifier,
            Code = role.RoleIdentifier,
            Urn = role.RoleIdentifier,
            EntityTypeId = state.OrganizationType.Id,
            ProviderId = state.Provider.Id,
        });
    }

    private async Task<JobResult?> Flush(State state, JobContext context, string currentPage, CancellationToken cancellationToken)
    {
        var result = await FlushTask;
        if (result is { })
        {
            return result;
        }

        var (roles, assignments) = state.ReadyForFlush();
        FlushTask = IngestEntites();
        return null;

        async Task<JobResult?> IngestEntites()
        {
            try
            {
                var audit = NewAudit();
                var upsertedRoles = await IngestService.IngestAndMergeData(roles, audit, RoleMergeMatchFilter, cancellationToken);
                var upsertedAssignments = await IngestService.IngestAndMergeData(assignments, audit, AssignmentMergeMatchFilter, cancellationToken);

                await context.Lease.UpsertLeastAndRefresh(state.Lease, content => content.NextPage = currentPage, cancellationToken);
            }
            catch (Exception ex)
            {
                return JobResult.Failure(ex);
            }

            return null;
        }
    }

    private class State
    {
        internal (List<Role> Roles, List<Assignment> Assignments) ReadyForFlush()
        {
            var roles = NewRoles;
            var assignments = NewAssignments;

            NewRoles = [];
            NewAssignments = [];
            IngestedRoles.AddRange(roles);

            return (roles, assignments);
        }

        internal static async Task<State> Create(AppDbContext dbContext, LeaseResult<LeaseContent> lease, CancellationToken cancellationToken)
        {
            var orgType = await dbContext.EntityTypes.FirstOrDefaultAsync(t => t.Name == "Organisasjon", cancellationToken);
            var provider = await dbContext.Providers.FirstAsync(p => p.Code == "ccr", cancellationToken);
            var roles = await dbContext.Roles.ToListAsync(cancellationToken);

            return new State()
            {
                Lease = lease,
                DbContext = dbContext,
                OrganizationType = orgType,
                Provider = provider,
                NewRoles = [],
                IngestedRoles = roles.ToList(),
            };
        }

        public LeaseResult<LeaseContent> Lease { get; set; }

        public AppDbContext DbContext { get; set; }

        public EntityType OrganizationType { get; init; }

        public Provider Provider { get; init; }

        public List<Role> IngestedRoles { get; private set; } = [];

        public List<Role> NewRoles { get; private set; } = [];

        public List<Role> AllRoles => IngestedRoles.Concat(NewRoles).ToList(); 

        public List<Assignment> NewAssignments { get; private set; } = [];
    }

    internal class LeaseContent
    {
        public string NextPage { get; set; }
    }
}
