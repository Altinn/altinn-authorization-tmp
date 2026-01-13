using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
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
    public async Task<Result<DelegationDto>> AddDelegationForAgentAsync(Guid partyId, Guid fromId, Guid toId, Guid? packageId, string package, CancellationToken cancellationToken = default)
    {
        if (PackageConstants.TryGetByName(package, out var pkg))
        {
        }
        else if (packageId.HasValue && PackageConstants.TryGetById((Guid)packageId, out pkg))
        {
        }
        else
        {
            return Problems.ConnectionEntitiesDoNotExist;
        }

        if (!pkg.Entity.IsAssignable)
        {
            return Problems.ConnectionEntitiesDoNotExist;
        }

        var possibleDelegationsTask = connectionQuery.GetConnectionsFromOthersAsync(
            new ConnectionQueryFilter
            {
                ToIds = [partyId],
                FromIds = [fromId],
                IncludeResource = true,
                IncludePackages = true,
                EnrichEntities = true,
            },
            ct: cancellationToken);
        var existingDelegationsTask = connectionQuery.GetConnectionsFromOthersAsync(
            new()
            {
                ViaIds = [partyId],
                ViaRoleIds = [RoleConstants.Agent],
                ToIds = [toId],
                FromIds = [fromId],
                EnrichEntities = true,
                IncludePackages = true,
            },
            true,
            cancellationToken);

        var possibleDelegations = await possibleDelegationsTask;
        var existingDelegations = await existingDelegationsTask;
        var assignmentToAgent = await db.Assignments
            .FirstOrDefaultAsync(a => a.FromId == partyId && a.ToId == toId, cancellationToken: cancellationToken);

        // Check if to has an agent releationship with party.
        if (assignmentToAgent is null)
        {
            return Problems.ConnectionEntitiesDoNotExist;
        }

        // Check if the delegations w eare actully tring to do exists.
        var existingDelegation = existingDelegations.FirstOrDefault(
            d => d.FromId == fromId &&
            d.ToId == toId &&
            d.ViaId == partyId &&
            d.Packages.Any(d => d.Id == pkg.Id));

        if (existingDelegation is { })
        {
            return DtoMapper.ConvertToDelegationDto(existingDelegation, pkg.Id);
        }

        // Check if client has assigned or delegated package to party. 
        var clientDelegationAssignment = possibleDelegations.Where(d => d.FromId == fromId && d.ToId == toId && d.Packages.Any(p => p.Id == pkg.Id)).ToList();
        if (clientDelegationAssignment.Count == 0)
        {
            return Problems.ConnectionEntitiesDoNotExist;
        }

        var delegation = new Delegation()
        {
            FromId = (Guid)clientDelegationAssignment.First(p => p.AssignmentId is { }).AssignmentId,
            ToId = assignmentToAgent.Id,
            FacilitatorId = partyId,
        };

        db.Delegations.Add(delegation);
        await db.SaveChangesAsync(cancellationToken);

        return DtoMapper.ConvertToDelegationDto(delegation, pkg.Id);
    }

    /// <inheritdoc/>
    public async Task<ProblemDescriptor?> RemoveAgentDelegation(Guid partyId, Guid fromId, Guid toId, Guid? packageId, string package, CancellationToken cancellationToken = default)
    {
        if (PackageConstants.TryGetByName(package, out var pkg))
        {
        }
        else if (packageId.HasValue && PackageConstants.TryGetById((Guid)packageId, out pkg))
        {
        }
        else
        {
            // error?
            return Problems.InvalidResourceCombination; 
        }

        if (!pkg.Entity.IsAssignable)
        {
            // error?
            return Problems.InvalidResourceCombination;
        }

        var existingDelegations = await connectionQuery.GetConnectionsFromOthersAsync(
            new()
            {
                ViaIds = [partyId],
                ViaRoleIds = [RoleConstants.Agent],
                ToIds = [toId],
                FromIds = [fromId],
                IncludePackages = true,
            },
            true,
            cancellationToken);

        var existingDelegation = existingDelegations.FirstOrDefault(
            d => d.FromId == fromId &&
            d.ToId == toId &&
            d.ViaId == partyId &&
            d.Packages.Any(d => d.Id == pkg.Id));

        var delegation = await db.Delegations.FirstOrDefaultAsync(d => d.Id == existingDelegation.DelegationId, cancellationToken);

        db.Delegations.Remove(delegation);
        await db.SaveChangesAsync(cancellationToken);

        return null;
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

    Task<Result<DelegationDto>> AddDelegationForAgentAsync(Guid partyId, Guid fromId, Guid toId, Guid? packageId, string package, CancellationToken cancellationToken = default);

    Task<ProblemDescriptor?> RemoveAgentDelegation(Guid partyId, Guid fromId, Guid toId, Guid? packageId, string package, CancellationToken cancellationToken = default);
}
