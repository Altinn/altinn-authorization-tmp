using System.Security.Cryptography;
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
using Microsoft.Extensions.Logging;

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

        var result = DtoMapper.ConvertToClientDelegationAgentDto(connections);

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
    public async Task<Result<List<ClientDto>>> GetDelegationsForClientAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default)
    {
        var delegations = await db.Assignments.AsNoTracking()
            .Where(a => a.FromId == partyId && a.ToId == toId && a.RoleId == RoleConstants.Agent.Id)
            .Join(
                db.Delegations,
                a => a.Id,
                d => d.ToId,
                (a, d) => new { Delegation = d })
                .Include(d => d.Delegation.From)
                .ThenInclude(d => d.From)
                .ThenInclude(d => d.Type)
                .Include(d => d.Delegation.From)
                .ThenInclude(d => d.From)
                .ThenInclude(d => d.Variant)
                .Include(d => d.Delegation)
                .ThenInclude(d => d.To)
                .Include(d => d)
                .ToListAsync(cancellationToken);

        var result = new List<ClientDto>();
        foreach (var delegation in delegations)
        {
            result.Append(DtoMapper.Convert(delegation))
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<AgentDto>>> GetDelegationsForAgentsAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default)
    {
        var delegations = connectionQuery
            .GetConnectionsAsync(new()
            {
                ViaIds = RoleConstants.Agent,
            });
    }

    /// <inheritdoc/>
    public Task<Result<DelegationDto>> AddDelegationForAgentAsync(Guid partyId, Guid fromId, Guid toId, Guid packageId, Guid package, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public Task RemoveAgentDelegation(Guid partyId, Guid fromId, Guid toId, Guid packageId, Guid package, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// a
/// </summary>
public interface IClientDelegationService
{
    Task<Result<List<ClientDto>>> GetClientsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<Result<List<AgentDto>>> GetAgentsForPartyAsync(Guid partyId, CancellationToken cancellationToken = default);

    Task<Result<AssignmentDto>> AddAgentForParty(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default);

    Task RemoveAgent(Guid partyId, Guid toUuid, CancellationToken cancellationToken = default);

    Task<Result<List<ClientDto>>> GetDelegationsForClientAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default);

    Task<Result<List<AgentDto>>> GetDelegationsForAgentsAsync(Guid partyId, Guid toId, CancellationToken cancellationToken = default);

    Task<Result<DelegationDto>> AddDelegationForAgentAsync(Guid partyId, Guid fromId, Guid toId, Guid packageId, Guid package, CancellationToken cancellationToken = default);

    Task RemoveAgentDelegation(Guid partyId, Guid fromId, Guid toId, Guid packageId, Guid package, CancellationToken cancellationToken = default);
}
