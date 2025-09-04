using System.Diagnostics;
using Altinn.AccessMgmt.Core.HostedServices.Contracts;
using Altinn.AccessMgmt.Core.HostedServices.Leases;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Utils;
using Altinn.Authorization.Host.Lease;
using Altinn.Authorization.Integration.Platform.Register;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Altinn.AccessMgmt.Core.HostedServices.Services;

/// <inheritdoc />
public class RoleSyncService : BaseSyncService, IRoleSyncService
{
    public RoleSyncService(
        IAltinnRegister register,
        ILogger<RoleSyncService> logger,
        IIngestService ingestService,
        IServiceProvider serviceProvider
    )
    {
        _register = register;
        _logger = logger;
        _ingestService = ingestService;
        _serviceProvider = serviceProvider;
    }

    private readonly IAltinnRegister _register;
    private readonly ILogger<RoleSyncService> _logger;
    private readonly IIngestService _ingestService;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public async Task SyncRoles(ILease lease, CancellationToken cancellationToken)
    {
        var batchData = new List<Assignment>();
        Guid batchId = Guid.CreateVersion7();
        var options = new AuditValues(
            AuditDefaults.RegisterImportSystem,
            AuditDefaults.RegisterImportSystem,
            Activity.Current?.TraceId.ToString() ?? Guid.CreateVersion7().ToString()
        );

        using var scope = _serviceProvider.CreateScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        OrgType = await appDbContext.EntityTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == "Organisasjon", cancellationToken);

        Provider = await appDbContext.Providers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Code == "ccr", cancellationToken);

        Roles = await appDbContext.Roles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var leaseData = await lease.Get<RegisterLease>(cancellationToken);

        await foreach (var page in await _register.StreamRoles([], leaseData.RoleStreamNextPageLink, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!page.IsSuccessful)
            {
                Log.ResponseError(_logger, page.StatusCode);
                throw new Exception("Stream page is not successful");
            }

            var batchName = batchId.ToString().ToLower().Replace("-", string.Empty);
            _logger.LogInformation("Starting proccessing role page '{0}'", batchName);

            if (page.Content != null)
            {
                foreach (var item in page.Content.Data)
                {
                    var assignment = await ConvertRoleModel(appDbContext, item, options: options, cancellationToken) ?? throw new Exception("Failed to convert RoleModel to Assignment");

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
                            await SetParent(appDbContext, assignment.FromId, assignment.ToId, options: options, cancellationToken: cancellationToken);
                        }
                    }
                    else
                    {
                        var deleteAssignment = await appDbContext.Assignments
                            .AsTracking()
                            .Where(a => a.FromId == assignment.FromId && a.ToId == assignment.ToId && a.RoleId == assignment.RoleId)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (deleteAssignment is { })
                        {
                            appDbContext.Remove(deleteAssignment);
                            await appDbContext.SaveChangesAsync(cancellationToken);
                        }

                        if (item.RoleIdentifier == "hovedenhet" || item.RoleIdentifier == "ikke-naeringsdrivende-hovedenhet")
                        {
                            await RemoveParent(appDbContext, assignment.FromId, options: options, cancellationToken: cancellationToken);
                        }
                    }
                }
            }

            await Flush(batchId);

            if (string.IsNullOrEmpty(page?.Content?.Links?.Next))
            {
                return;
            }

            leaseData.RoleStreamNextPageLink = page.Content.Links.Next;
            await lease.Update(leaseData, cancellationToken);

            //// await Flush(batchId);

            async Task Flush(Guid batchId)
            {
                try
                {
                    _logger.LogInformation("Ingest and Merge Assignment batch '{0}' to db", batchId.ToString());
                    var ingested = await _ingestService.IngestTempData<Assignment>(batchData, batchId, cancellationToken);

                    if (ingested != batchData.Count)
                     {
                        _logger.LogWarning("Ingest partial complete: Assignment ({0}/{1})", ingested, batchData.Count);
                    }

                    var merged = await _ingestService.MergeTempData<Assignment>(batchId, options, ["fromid", "roleid", "toid"], cancellationToken: cancellationToken);

                    _logger.LogInformation("Merge complete: Assignment ({0}/{1})", merged, ingested);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to ingest and/or merge Assignment and EntityLookup batch {0} to db", batchId.ToString());
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

    private async Task SetParent(AppDbContext dbContext, Guid childId, Guid parentId, AuditValues options, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await dbContext.Entities
                .AsTracking()
                .FirstAsync(e => e.Id == childId, cancellationToken: cancellationToken);

            entity.ParentId = parentId;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Unable to set '{1}' as parent to '{0}'", childId, parentId), ex);
        }
    }

    private async Task RemoveParent(AppDbContext dbContext, Guid childId, AuditValues options, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await dbContext.Entities
                .AsTracking()
                .FirstAsync(e => e.Id == childId, cancellationToken: cancellationToken);

            entity.ParentId = null;
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Unable to remove parent for '{0}'", childId), ex);
        }
    }

    private static readonly IReadOnlyList<string> GetAssignmentMergeMatchFilter = new List<string>() { "fromid", "roleid", "toid" }.AsReadOnly();

    private List<Role> Roles { get; set; } = [];

    private async Task<Assignment> ConvertRoleModel(AppDbContext dbContext, RoleModel model, AuditValues options, CancellationToken cancellationToken)
    {
        try
        {
            var role = await GetOrCreateRole(dbContext, model.RoleIdentifier, model.RoleSource, options: options, cancellationToken);
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

    private async Task<Role> GetOrCreateRole(AppDbContext dbContext, string roleIdentifier, string roleSource, AuditValues options, CancellationToken cancellationToken)
    {
        if (Roles.Count(t => t.Code == roleIdentifier) == 1)
        {
            return Roles.First(t => t.Code == roleIdentifier);
        }

        var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Code == roleIdentifier, cancellationToken);
        if (role is null)
        {
            dbContext.Add(new Role()
            {
                Id = Guid.CreateVersion7(),
                Name = roleIdentifier,
                Description = roleIdentifier,
                Code = roleIdentifier,
                Urn = roleIdentifier,
                EntityTypeId = OrgType.Id,
                ProviderId = Provider.Id,
            });
            await dbContext.SaveChangesAsync(cancellationToken);
            role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Code == roleIdentifier, cancellationToken);

            if (role is null)
            {
                throw new Exception(string.Format("Unable to get or create role '{0}'", roleIdentifier));
            }
        }

        Roles.Add(role);
        return role;
    }

    private EntityType OrgType { get; set; }

    private Provider Provider { get; set; }
}
