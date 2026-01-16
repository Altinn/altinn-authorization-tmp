using System.Collections.Immutable;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class ClientDelegationService(
    AppDbContext db,
    ConnectionQuery connectionQuery) : IClientDelegationService
{
    /// <inheritdoc/>
    public async Task<Result<List<AgentDto>>> GetAgentsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var connections = await connectionQuery.GetConnectionsToOthersAsync(
            new ConnectionQueryFilter
            {
                ToIds = [],
                FromIds = [partyId],
                RoleIds = [RoleConstants.Agent],
                IncludeResource = true,
                IncludePackages = true,
                EnrichEntities = true,
            },
            ct: cancellationToken);

        var result = DtoMapper.ConvertToAgentDto(connections);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<List<ClientDto>>> GetClientsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default)
    {
        var connections = await connectionQuery.GetConnectionsFromOthersAsync(
            new ConnectionQueryFilter
            {
                ToIds = [partyId],
                FromIds = [],
                IncludeResource = true,
                IncludePackages = true,
                EnrichEntities = true,
            },
            ct: cancellationToken);

        var result = DtoMapper.ConvertToClientDto(connections);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<AssignmentDto>> AddAgentForParty(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await db.Assignments.AsNoTracking().Where(p => p.FromId == partyId && p.ToId == toUuid && p.RoleId == RoleConstants.Agent).FirstOrDefaultAsync(cancellationToken);
        if (existingAssignment is { })
        {
            return DtoMapper.Convert(existingAssignment);
        }

        var entity = await db.Entities.FirstOrDefaultAsync(e => e.Id == toUuid, cancellationToken);
        if (entity is null)
        {
            return Problems.EntityTypeNotFound;
        }

        if ((entity.TypeId != EntityTypeConstants.Person) && entity.TypeId != EntityTypeConstants.SystemUser && entity.VariantId == EntityVariantConstants.AgentSystem)
        {
            return Problems.UnsupportedEntityType;
        }

        var assignment = new Assignment
        {
            FromId = partyId,
            ToId = toUuid,
            RoleId = RoleConstants.Agent,
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(cancellationToken);

        return DtoMapper.Convert(assignment);
    }

    /// <inheritdoc/>
    public async Task RemoveAgent(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default)
    {
        var existingAssignment = await db.Assignments.AsTracking().Where(p => p.FromId == partyId && p.ToId == toUuid && p.RoleId == RoleConstants.Agent).FirstOrDefaultAsync(cancellationToken);
        if (existingAssignment is null)
        {
            return;
        }

        db.Assignments.Remove(existingAssignment);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<List<AgentDto>>> GetDelegatedAccessPackagesFromClientsViaParty(Guid partyId, Guid fromId, CancellationToken cancellationToken = default)
    {
        var connections = await connectionQuery.GetConnectionsToOthersAsync(
            new()
            {
                FromIds = [fromId],
                ViaIds = [partyId],
                ViaRoleIds = [RoleConstants.Agent],
                EnrichEntities = true,
                IncludePackages = true
            },
            true,
            cancellationToken);

        var result = DtoMapper.ConvertToAgentDto(connections);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<List<ClientDto>>> GetDelegatedAccessPackagesToAgentsViaPartyAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default)
    {
        var connections = await connectionQuery.GetConnectionsFromOthersAsync(
            new()
            {
                ViaIds = [partyId],
                ViaRoleIds = [RoleConstants.Agent],
                ToIds = [toId],
                EnrichEntities = true,
                IncludePackages = true,
            },
            true,
            cancellationToken);

        var result = DtoMapper.ConvertToClientDto(connections);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<List<DelegationDto>>> AddDelegationForAgentAsync(
        Guid partyId,
        Guid fromId,
        Guid toId,
        DelegationBatchInputDto payload,
        CancellationToken cancellationToken = default)
    {
        var inputs = payload.Values
            .Select(r =>
            {
                var roleExist = RoleConstants.TryGetByCode(r.Role, out var role);

                var pkgs = new List<(bool PackageExist, Package Package, string InputPackage)>();
                foreach (var packageInput in r.Packages)
                {
                    var packageExist = PackageConstants.TryGetByUrn(packageInput, out var package);
                    pkgs.Add(new()
                    {
                        InputPackage = packageInput,
                        Package = package,
                        PackageExist = packageExist,
                    });
                }

                pkgs = pkgs.DistinctBy(p => p.Package?.Id).ToList();

                return new
                {
                    InputRole = r.Role,
                    InputPackage = r.Packages,
                    InputOk = roleExist && pkgs.All(p => p.PackageExist),
                    Role = role,
                    Packages = pkgs,
                };
            });

        if (inputs.Any(i => !i.InputOk))
        {
            // Return different err
            return Problems.ConnectionEntitiesDoNotExist;
        }

        var agentAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.FromId == partyId && a.ToId == toId && a.RoleId == RoleConstants.Agent, cancellationToken: cancellationToken);
        var result = new List<DelegationDto>();
        foreach (var input in inputs)
        {
            var pkgIds = input.Packages.Select(p => p.Package).Select(p => p.Id).Distinct();
            var clientAssignment = await db.Assignments.AsNoTracking().FirstOrDefaultAsync(t => t.FromId == fromId && t.ToId == partyId && t.RoleId == input.Role.Id, cancellationToken);

            // check ass must exist
            var rolePackages = await db.RolePackages.AsNoTracking().Where(t => t.RoleId == input.Role.Id && pkgIds.Contains(t.PackageId)).ToListAsync(cancellationToken);
            var assignmentPackages = await db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == clientAssignment.Id && pkgIds.Contains(t.PackageId)).ToListAsync(cancellationToken);

            var delegation = await db.Delegations.AsNoTracking().FirstOrDefaultAsync(t => t.FromId == clientAssignment.Id && t.ToId == agentAssignment.Id && t.FacilitatorId == partyId, cancellationToken);
            if (delegation is null)
            {
                delegation = new Delegation()
                {
                    FromId = clientAssignment.Id,
                    ToId = agentAssignment.Id,
                    FacilitatorId = partyId
                };

                db.Delegations.Add(delegation);
            }

            var existingDelegationPackages = db.DelegationPackages.Where(t => t.DelegationId == delegation.Id);
            foreach (var pkgId in pkgIds)
            {
                var rolePackageId = rolePackages.FirstOrDefault(r => r.PackageId == pkgId)?.Id;
                var assignmentPackageId = assignmentPackages.FirstOrDefault(t => t.PackageId == pkgId)?.Id;

                if (!existingDelegationPackages.Any(
                    t =>
                    t.DelegationId == delegation.Id &&
                    t.PackageId == pkgId &&
                    t.RolePackageId == rolePackageId &&
                    t.AssignmentPackageId == assignmentPackageId))
                {
                    db.DelegationPackages.Add(new DelegationPackage()
                    {
                        PackageId = pkgId,
                        DelegationId = delegation.Id,
                        RolePackageId = rolePackageId,
                        AssignmentPackageId = assignmentPackageId,
                    });
                }

                result.Add(new()
                {
                    FromId = fromId,
                    ToId = toId,
                    ViaId = partyId,
                    RoleId = input.Role,
                    PackageId = pkgId,
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<List<DelegationDto>>> RemoveAgentDelegation(
        Guid partyId,
        Guid fromId,
        Guid toId,
        DelegationBatchInputDto payload,
        CancellationToken cancellationToken = default)
    {
        var inputs = payload.Values
            .Select(r =>
            {
                var roleExist = RoleConstants.TryGetByCode(r.Role, out var role);

                var pkgs = new List<(bool PackageExist, Package Package, string InputPackage)>();
                foreach (var packageInput in r.Packages)
                {
                    var packageExist = PackageConstants.TryGetByUrn(packageInput, out var package);
                    pkgs.Add(new()
                    {
                        InputPackage = packageInput,
                        Package = package,
                        PackageExist = packageExist,
                    });
                }
                
                pkgs = pkgs.DistinctBy(p => p.Package?.Id).ToList();

                return new
                {
                    InputRole = r.Role,
                    InputPackage = r.Packages,
                    InputOk = roleExist && pkgs.All(p => p.PackageExist),
                    Role = role,
                    Packages = pkgs,
                };
            });

        if (inputs.Any(i => !i.InputOk))
        {
            // Return different err
            return Problems.ConnectionEntitiesDoNotExist;
        }

        var agentAssignment = await db.Assignments
            .FirstOrDefaultAsync(a => a.FromId == partyId && a.ToId == toId && a.RoleId == RoleConstants.Agent, cancellationToken: cancellationToken);
        var result = new List<DelegationDto>();
        foreach (var input in inputs)
        {
            var pkgIds = input.Packages.Select(p => p.Package).Select(p => p.Id).Distinct();
            var clientAssignment = await db.Assignments.AsNoTracking().FirstOrDefaultAsync(t => t.FromId == fromId && t.ToId == partyId && t.RoleId == input.Role.Id, cancellationToken);

            // check ass must exist
            var rolePackages = await db.RolePackages.AsNoTracking().Where(t => t.RoleId == input.Role.Id && pkgIds.Contains(t.PackageId)).ToListAsync(cancellationToken);
            var assignmentPackages = await db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == clientAssignment.Id && pkgIds.Contains(t.PackageId)).ToListAsync(cancellationToken);

            var delegation = await db.Delegations.AsNoTracking().FirstOrDefaultAsync(t => t.FromId == clientAssignment.Id && t.ToId == agentAssignment.Id && t.FacilitatorId == partyId, cancellationToken);
            if (delegation is null)
            {
                delegation = new Delegation()
                {
                    FromId = clientAssignment.Id,
                    ToId = agentAssignment.Id,
                    FacilitatorId = partyId
                };

                db.Delegations.Add(delegation);
            }

            var existingDelegationPackages = await db.DelegationPackages.AsTracking().Where(t => t.DelegationId == delegation.Id).ToListAsync(cancellationToken);
            foreach (var pkgId in pkgIds)
            {
                var rolePackageId = rolePackages.FirstOrDefault(r => r.PackageId == pkgId)?.Id;
                var assignmentPackageId = assignmentPackages.FirstOrDefault(t => t.PackageId == pkgId)?.Id;

                var toRemove = existingDelegationPackages.FirstOrDefault(
                    t =>
                    t.DelegationId == delegation.Id &&
                    t.PackageId == pkgId &&
                    t.RolePackageId == rolePackageId &&
                    t.AssignmentPackageId == assignmentPackageId);

                if (toRemove is { })
                {
                    db.DelegationPackages.Remove(toRemove);

                    result.Add(new()
                    {
                        FromId = fromId,
                        ToId = toId,
                        ViaId = partyId,
                        RoleId = input.Role,
                        PackageId = pkgId,
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return result;
    }
}

/// <summary>
/// I
/// </summary>
public interface IClientDelegationService
{
    Task<Result<List<ClientDto>>> GetClientsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<Result<List<AgentDto>>> GetAgentsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<Result<AssignmentDto>> AddAgentForParty(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default);

    Task RemoveAgent(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default);

    Task<Result<List<AgentDto>>> GetDelegatedAccessPackagesFromClientsViaParty(Guid partyId, Guid fromId, CancellationToken cancellationToken = default);

    Task<Result<List<ClientDto>>> GetDelegatedAccessPackagesToAgentsViaPartyAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default);

    Task<Result<List<DelegationDto>>> AddDelegationForAgentAsync(Guid partyId, Guid fromId, Guid toId, DelegationBatchInputDto payload, CancellationToken cancellationToken = default);

    Task<Result<List<DelegationDto>>> RemoveAgentDelegation(Guid partyId, Guid fromId, Guid toId, DelegationBatchInputDto payload, CancellationToken cancellationToken = default);
}
