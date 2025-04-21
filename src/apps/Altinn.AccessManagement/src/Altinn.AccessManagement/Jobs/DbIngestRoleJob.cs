using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Helpers;
using Altinn.AccessMgmt.Persistence.Core.Models;
using Altinn.AccessMgmt.Persistence.Data;
using Altinn.AccessMgmt.Persistence.Repositories.Contracts;
using Altinn.AccessMgmt.Persistence.Services;
using Altinn.Authorization.Host.Job;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;

namespace Altinn.AccessManagement.HostedServices.Jobs;

public class DbIngestRoleJob(
    IAltinnRegister register,
    IIngestService ingestService,
    IRoleRepository roleRepository,
    IProviderRepository providerRepository,
    IAssignmentRepository assignmentRepository,
    IStatusService statusService,
    IEntityRepository entityRepository,
    IEntityTypeRepository entityTypeRepository
) : IJob
{
    private readonly IAltinnRegister Register = register;

    private readonly IRoleRepository RoleRepository = roleRepository;

    private readonly IProviderRepository ProviderRepository = providerRepository;

    private readonly IAssignmentRepository AssignmentRepository = assignmentRepository;

    private readonly IStatusService StatusService = statusService;

    private readonly IEntityRepository EntityRepository = entityRepository;

    private readonly IEntityTypeRepository EntityTypeRepository = entityTypeRepository;

    private ChangeRequestOptions Audit => new()
    {
        ChangedBy = AuditDefaults.RegisterImportSystem,
        ChangedBySystem = AuditDefaults.RegisterImportSystem,
    };

    public async Task<bool> CanRun(JobContext context, CancellationToken cancellationToken)
    {
        var roleStatus = await StatusService.GetOrCreateRecord(Guid.Parse("84E9726D-E61B-4DFF-91D7-9E17C8BB41A6"), "accessmgmt-sync-register-role", Audit, 5);
        return await StatusService.TryToRun(roleStatus, Audit, cancellationToken);
    }

    public async Task<JobResult> Run(JobContext context, CancellationToken cancellationToken)
    {
        var batchData = new List<Assignment>();
        Guid batchId = Guid.CreateVersion7();

        var options = new ChangeRequestOptions()
        {
            ChangedBy = AuditDefaults.RegisterImportSystem,
            ChangedBySystem = AuditDefaults.RegisterImportSystem
        };

        OrgType = (await EntityTypeRepository.Get(t => t.Name, "Organisasjon")).FirstOrDefault();
        Provider = (await ProviderRepository.Get(t => t.Code, "ccr")).FirstOrDefault();
        using var ls = await context.Lease.TryAquireNonBlocking<LeaseContent>("lease_name", cancellationToken);
        await foreach (var page in await Register.StreamRoles([], ls.Data?.NextPageLink, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return JobResult.Success();
            }

            if (!page.IsSuccessful)
            {
                return JobResult.Failure();
            }

            options.ChangeOperationId = batchId;
            var batchName = batchId.ToString().ToLower().Replace("-", string.Empty);

            if (page.Content != null)
            {
                foreach (var item in page.Content.Data)
                {
                    var assignment = await ConvertRoleModel(item, options: options) ?? throw new Exception("Failed to convert RoleModel to Assignment");

                    if (batchData.Any(t => t.FromId == assignment.FromId && t.ToId == assignment.ToId && t.RoleId == assignment.RoleId))
                    {
                        // If changes on same assignment then execute as-is before continuing.
                        await Flush(batchId);
                    }

                    if (item.Type == "Added")
                    {
                        batchData.Add(assignment);
                        if (item.RoleIdentifier == "hovedenhet" || item.RoleIdentifier == "ikke-naeringsdrivende-hovedenhet")
                        {
                            await SetParent(assignment.FromId, assignment.ToId, options: options, cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        var filter = AssignmentRepository.CreateFilterBuilder();
                        filter.Equal(t => t.FromId, assignment.FromId);
                        filter.Equal(t => t.ToId, assignment.ToId);
                        filter.Equal(t => t.RoleId, assignment.RoleId);
                        await AssignmentRepository.Delete(filter, options: options, cancellationToken: cancellationToken);

                        if (item.RoleIdentifier == "hovedenhet" || item.RoleIdentifier == "ikke-naeringsdrivende-hovedenhet")
                        {
                            await RemoveParent(assignment.FromId, options: options, cancellationToken: cancellationToken);

                        }
                    }
                }

                await Flush(batchId);

                await UpdateLease(context, ls, data => data.NextPageLink = page.Content.Links.Next, cancellationToken);
                if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
                {
                    return JobResult.Success();
                }

                await Flush(batchId);

                async Task Flush(Guid batchId)
                {
                    try
                    {
                        // Logger.LogInformation("Ingest and Merge Assignment batch '{0}' to db", batchId.ToString());
                        var ingested = await ingestService.IngestTempData<Assignment>(batchData, batchId, options: options, cancellationToken: cancellationToken);

                        if (ingested != batchData.Count)
                        {
                            // Logger.LogWarning("Ingest partial complete: Assignment ({0}/{1})", ingested, batchData.Count);
                        }

                        var merged = await ingestService.MergeTempData<Assignment>(batchId, options: options, GetAssignmentMergeMatchFilter, cancellationToken: cancellationToken);

                        // Logger.LogInformation("Merge complete: Assignment ({0}/{1})", merged, ingested);
                    }
                    catch (Exception ex)
                    {
                        // Logger.LogError(ex, "Failed to ingest and/or merge Assignment and EntityLookup batch {0} to db", batchId.ToString());
                        throw new Exception(string.Format("Failed to ingest and/or merge Assignment and EntityLookup batch {0} to db", batchId.ToString()), ex);
                    }
                    finally
                    {
                        batchId = Guid.CreateVersion7();
                        batchData.Clear();
                    }
                }
            }
        }

        return JobResult.Success();
    }

    private async Task UpdateLease(JobContext jobContext, LeaseResult<LeaseContent> ls, Action<LeaseContent> configureLeaseContent, CancellationToken cancellationToken)
    {
        configureLeaseContent(ls.Data);
        await jobContext.Lease.Put(ls, ls.Data, cancellationToken);
        await jobContext.Lease.RefreshLease(ls, cancellationToken);
    }

    private async Task SetParent(Guid childId, Guid parentId, ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await EntityRepository.Update(t => t.ParentId, parentId, childId, options: options, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Unable to set '{1}' as parent to '{0}'", childId, parentId), ex);
        }
    }

    private async Task RemoveParent(Guid childId, ChangeRequestOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await EntityRepository.Update(t => t.ParentId, childId, options, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Unable to remove parent for '{0}'", childId), ex);
        }
    }

    private static readonly IReadOnlyList<string> GetAssignmentMergeMatchFilter = new List<string>() { "fromid", "roleid", "toid" }.AsReadOnly();

    private List<Role> Roles { get; set; } = [];

    private async Task<Assignment> ConvertRoleModel(RoleModel model, ChangeRequestOptions options)
    {
        try
        {
            var role = await GetOrCreateRole(model.RoleIdentifier, model.RoleSource, options: options);
            return new Assignment()
            {
                FromId = Guid.Parse(model.FromParty),
                ToId = Guid.Parse(model.ToParty),
                RoleId = role.Id
            };
        }
        catch
        {
            throw new Exception(string.Format("Failed to convert model to Assignment. From:{0} To:{1} Role:{2}", model.FromParty, model.ToParty, model.RoleIdentifier));
        }
    }

    private async Task<Role> GetOrCreateRole(string roleIdentifier, string roleSource, ChangeRequestOptions options)
    {
        if (Roles.Count(t => t.Code == roleIdentifier) == 1)
        {
            return Roles.First(t => t.Code == roleIdentifier);
        }

        var role = (await RoleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
        if (role == null)
        {
            await RoleRepository.Create(
                new Role()
                {
                    Id = Guid.CreateVersion7(),
                    Name = roleIdentifier,
                    Description = roleIdentifier,
                    Code = roleIdentifier,
                    Urn = roleIdentifier,
                    EntityTypeId = OrgType.Id,
                    ProviderId = Provider.Id,
                },
                options: options
            );

            role = (await RoleRepository.Get(t => t.Urn, roleIdentifier)).FirstOrDefault();
            if (role == null)
            {
                throw new Exception(string.Format("Unable to get or create role '{0}'", roleIdentifier));
            }
        }

        Roles.Add(role);
        return role;
    }

    private EntityType OrgType { get; set; }

    private Provider Provider { get; set; }

    private List<GenericFilter> assignmentMergeFilter =
    [
        new GenericFilter("fromid", "fromid"),
        new GenericFilter("toid", "toid"),
        new GenericFilter("roleid", "roleid"),
    ];

    public class LeaseContent
    {
        /// <summary>
        /// The URL of the next page of AssignmentSuccess data.
        /// </summary>
        public string NextPageLink { get; set; }
    }
}
