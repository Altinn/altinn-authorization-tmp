using System.ComponentModel.DataAnnotations;
using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Audit;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services;

/// <summary>
/// Service for managing Maskinporten supplier relationships and scope delegations
/// </summary>
public class MaskinportenSupplierService(
    AppDbContext dbContext,
    ConnectionQuery connectionQuery,
    IAuditAccessor auditAccessor,
    IConnectionService connectionService) : IMaskinportenSupplierService
{
    private const string MaskinportenSchemaTypeName = "MaskinportenSchema";

    /// <inheritdoc />
    public async Task<Result<AssignmentDto>> AddSupplier(Guid consumerId, Guid supplierId, CancellationToken cancellationToken = default)
    {
        var (consumer, supplier) = await GetAndValidateOrganizations(consumerId, supplierId, cancellationToken);
        if (consumer.IsProblem)
        {
            return consumer.Problem;
        }

        // Check if assignment already exists
        var existingAssignment = await dbContext.Assignments
            .AsNoTracking()
            .Where(e => e.FromId == consumerId)
            .Where(e => e.ToId == supplierId)
            .Where(e => e.RoleId == RoleConstants.Supplier.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignment is { })
        {
            return DtoMapper.Convert(existingAssignment);
        }

        // Create new supplier assignment
        var assignment = new Assignment()
        {
            FromId = consumerId,
            ToId = supplierId,
            RoleId = RoleConstants.Supplier.Id,
        };

        await dbContext.Assignments.AddAsync(assignment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return DtoMapper.Convert(assignment);
    }

    /// <inheritdoc />
    public async Task<ValidationProblemInstance> RemoveSupplier(Guid consumerId, Guid supplierId, bool cascade = false, CancellationToken cancellationToken = default)
    {
        var (validation, _) = await GetAndValidateOrganizations(consumerId, supplierId, cancellationToken);
        if (validation.IsProblem)
        {
            return validation.Problem as ValidationProblemInstance;
        }

        var existingAssignment = await dbContext.Assignments
            .AsTracking()
            .Where(e => e.FromId == consumerId)
            .Where(e => e.ToId == supplierId)
            .Where(e => e.RoleId == RoleConstants.Supplier.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingAssignment is null)
        {
            return null;
        }

        if (!cascade)
        {
            var assignedResources = await dbContext.AssignmentResources
                .AsNoTracking()
                .Where(p => p.AssignmentId == existingAssignment.Id)
                .ToListAsync(cancellationToken);

            var problem = ValidationComposer.Validate(
                ResourceValidation.HasAssignedResources(assignedResources)
            );

            if (problem is { })
            {
                return problem;
            }
        }

        dbContext.Remove(existingAssignment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return null;
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ConnectionDto>>> GetSuppliers(Guid consumerId, Guid? supplierId = null, CancellationToken cancellationToken = default)
    {
        var consumer = await GetEntity(consumerId, cancellationToken);
        if (consumer.IsProblem)
        {
            return consumer.Problem;
        }

        var connections = await connectionQuery.GetConnectionsAsync(
            new ConnectionQueryFilter()
            {
                FromIds = [consumerId],
                ToIds = supplierId.HasValue ? [supplierId.Value] : null,
                RoleIds = [RoleConstants.Supplier.Id],
                EnrichEntities = true,
                IncludeKeyRole = true,
                ExcludeDeleted = false,
            },
            ConnectionQueryDirection.ToOthers,
            true,
            cancellationToken
        );

        return DtoMapper.ConvertToOthers(connections, getSingle: supplierId.HasValue);
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ConnectionDto>>> GetConsumers(Guid supplierId, Guid? consumerId = null, CancellationToken cancellationToken = default)
    {
        var supplier = await GetEntity(supplierId, cancellationToken);
        if (supplier.IsProblem)
        {
            return supplier.Problem;
        }

        var connections = await connectionQuery.GetConnectionsAsync(
            new ConnectionQueryFilter()
            {
                FromIds = consumerId.HasValue ? [consumerId.Value] : null,
                ToIds = [supplierId],
                RoleIds = [RoleConstants.Supplier.Id],
                EnrichEntities = true,
                IncludeKeyRole = true,
                ExcludeDeleted = false,
            },
            ConnectionQueryDirection.FromOthers,
            true,
            cancellationToken
        );

        return DtoMapper.ConvertFromOthers(connections, getSingle: consumerId.HasValue);
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ResourcePermissionDto>>> GetSupplierResources(
        Guid consumerId,
        Guid? supplierId = null,
        Guid? resourceId = null,
        string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var consumer = await GetEntity(consumerId, cancellationToken);
        if (consumer.IsProblem)
        {
            return consumer.Problem;
        }

        // Build resource filter (by ID or scope)
        List<Guid>? resourceIds = null;
        if (resourceId.HasValue)
        {
            resourceIds = [resourceId.Value];
        }
        else if (!string.IsNullOrWhiteSpace(scope))
        {
            resourceIds = await GetResourceIdsByScope(scope, cancellationToken);
            if (resourceIds.Count == 0)
            {
                return new List<ResourcePermissionDto>(); // No matching resources
            }
        }

        var connections = await connectionQuery.GetConnectionsAsync(
            new ConnectionQueryFilter()
            {
                FromIds = [consumerId],
                ToIds = supplierId.HasValue ? [supplierId.Value] : null,
                RoleIds = [RoleConstants.Supplier.Id],
                ResourceIds = resourceIds,
                IncludeResources = true,
                IncludeKeyRole = true,
                EnrichEntities = true,
            },
            ConnectionQueryDirection.ToOthers,
            true,
            cancellationToken
        );

        var resources = DtoMapper.ConvertResources(connections);

        // Filter to only MaskinportenSchema resources
        return resources.Where(r => r.Resource.Type?.Name == MaskinportenSchemaTypeName).ToList();
    }

    /// <inheritdoc />
    public async Task<Result<IEnumerable<ResourcePermissionDto>>> GetConsumerResources(
        Guid supplierId,
        Guid? consumerId = null,
        Guid? resourceId = null,
        string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var supplier = await GetEntity(supplierId, cancellationToken);
        if (supplier.IsProblem)
        {
            return supplier.Problem;
        }

        // Build resource filter (by ID or scope)
        List<Guid>? resourceIds = null;
        if (resourceId.HasValue)
        {
            resourceIds = [resourceId.Value];
        }
        else if (!string.IsNullOrWhiteSpace(scope))
        {
            resourceIds = await GetResourceIdsByScope(scope, cancellationToken);
            if (resourceIds.Count == 0)
            {
                return new List<ResourcePermissionDto>();
            }
        }

        var connections = await connectionQuery.GetConnectionsAsync(
            new ConnectionQueryFilter()
            {
                FromIds = consumerId.HasValue ? [consumerId.Value] : null,
                ToIds = [supplierId],
                RoleIds = [RoleConstants.Supplier.Id],
                ResourceIds = resourceIds,
                IncludeResources = true,
                IncludeKeyRole = true,
                EnrichEntities = true,
            },
            ConnectionQueryDirection.FromOthers,
            true,
            cancellationToken
        );

        var resources = DtoMapper.ConvertResources(connections);

        // Filter to only MaskinportenSchema resources
        return resources.Where(r => r.Resource.Type?.Name == MaskinportenSchemaTypeName).ToList();
    }

    /// <inheritdoc />
    public async Task<Result<ResourceCheckDto>> ResourceDelegationCheck(
        Guid consumerId,
        string resource,
        string languageCode = "nb",
        CancellationToken cancellationToken = default)
    {
        var consumer = await GetEntity(consumerId, cancellationToken);
        if (consumer.IsProblem)
        {
            return consumer.Problem;
        }

        if (!IsOrganization(consumer.Value))
        {
            return Problems.PartyNotFound;
        }

        // Validate that the resource is a MaskinportenSchema
        var resourceObj = await dbContext.Resources
            .Include(r => r.Type)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RefId == resource, cancellationToken);

        if (resourceObj == null || !resourceObj.Type.Name.Equals(MaskinportenSchemaTypeName, StringComparison.InvariantCultureIgnoreCase))
        {
            return Problems.InvalidResource;
        }

        // Use authenticated user from audit context
        var authenticatedUserId = auditAccessor.AuditValues.ChangedBy;

        // Delegate to ConnectionService with allowMaskinportenSchema = true for MaskinportenSchema resources
        // This allows delegation even if delegable=false, but still requires valid access rights
        return await connectionService.ResourceDelegationCheck(
            authenticatedUserId,
            consumerId,
            resource,
            configureConnection: null,
            languageCode: languageCode,
            ignoreDelegableFlag: false,
            allowMaskinportenSchema: true,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Result<bool>> AddResource(
        Guid consumerId,
        Guid supplierId,
        string resource,
        CancellationToken cancellationToken = default)
    {
        var (validation, entities) = await GetAndValidateOrganizations(consumerId, supplierId, cancellationToken);
        if (validation.IsProblem)
        {
            return validation.Problem;
        }

        // Validate resource is MaskinportenSchema
        var resourceObj = await dbContext.Resources
            .Include(r => r.Type)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RefId == resource, cancellationToken);

        if (resourceObj == null)
        {
            return ValidationComposer.Validate(
                ResourceValidation.ResourceExists(resourceObj, resource)
            );
        }

        if (!resourceObj.Type.Name.Equals(MaskinportenSchemaTypeName, StringComparison.InvariantCultureIgnoreCase))
        {
            return Problems.InvalidResource;
        }

        // Get authenticated user
        var by = await GetEntity(auditAccessor.AuditValues.ChangedBy, cancellationToken);
        if (by.IsProblem)
        {
            return by.Problem;
        }

        // Perform delegation check
        var delegationCheck = await ResourceDelegationCheck(consumerId, resource, cancellationToken: cancellationToken);
        if (delegationCheck.IsProblem)
        {
            return delegationCheck.Problem;
        }

        // Get all delegable rights from the delegation check
        var delegableRightKeys = delegationCheck.Value.Rights
            .Where(r => r.Result)
            .Select(r => r.Right.Key)
            .ToList();

        if (!delegableRightKeys.Any())
        {
            return Problems.NotAuthorizedForDelegationRequest;
        }

        // Ensure supplier assignment exists
        var connection = await GetSuppliers(consumerId, supplierId, cancellationToken);
        if (!connection.IsSuccess || !connection.Value.Any())
        {
            return Problems.MissingConnection;
        }

        // Delegate all available rights
        return await connectionService.AddResource(
            entities.Consumer,
            entities.Supplier,
            resourceObj,
            new RightKeyListDto { DirectRightKeys = delegableRightKeys },
            by.Value,
            configureConnection: null,
            cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ValidationProblemInstance> RemoveResource(
        Guid consumerId,
        Guid supplierId,
        string resource,
        CancellationToken cancellationToken = default)
    {
        var (validation, _) = await GetAndValidateOrganizations(consumerId, supplierId, cancellationToken);
        if (validation.IsProblem)
        {
            return validation.Problem as ValidationProblemInstance;
        }

        // Validate resource exists and is MaskinportenSchema
        var resourceObj = await dbContext.Resources
            .Include(r => r.Type)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RefId == resource, cancellationToken);

        if (resourceObj == null)
        {
            return ValidationComposer.Validate(
                ResourceValidation.ResourceExists(resourceObj, resource)
            );
        }

        if (!resourceObj.Type.Name.Equals(MaskinportenSchemaTypeName, StringComparison.InvariantCultureIgnoreCase))
        {
            return ValidationComposer.Validate(
                ResourceValidation.ResourceTypeIs(resourceObj, MaskinportenSchemaTypeName)
            );
        }

        return await connectionService.RemoveResource(
            consumerId,
            supplierId,
            resourceObj.Id,
            configureConnection: null,
            cancellationToken: cancellationToken);
    }

    #region Private Helper Methods

    private async Task<Result<Entity>> GetEntity(Guid entityId, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Entities
            .AsNoTracking()
            .Include(e => e.Type)
            .FirstOrDefaultAsync(e => e.Id == entityId, cancellationToken);

        if (entity is null)
        {
            return Problems.PartyNotFound;
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<Result<Entity>> GetEntity(string organizationNumber, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Entities
            .AsNoTracking()
            .Include(e => e.Type)
            .Where(e => e.RefId == organizationNumber)
            .Where(e => e.TypeId == EntityTypeConstants.Organization.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
        {
            return Problems.PartyNotFound;
        }

        return entity;
    }

    /// <inheritdoc />
    public async Task<Result<Resource>> GetResourceByRefId(string resourceRefId, CancellationToken cancellationToken = default)
    {
        var resource = await dbContext.Resources
            .AsNoTracking()
            .Include(r => r.Type)
            .FirstOrDefaultAsync(r => r.RefId == resourceRefId, cancellationToken);

        if (resource is null)
        {
            return Problems.InvalidResource;
        }

        return resource;
    }

    private async Task<(Result<bool> Validation, (Entity Consumer, Entity Supplier))> GetAndValidateOrganizations(
        Guid consumerId,
        Guid supplierId,
        CancellationToken cancellationToken)
    {
        var entities = await dbContext.Entities
            .AsNoTracking()
            .Where(e => e.Id == consumerId || e.Id == supplierId)
            .Include(e => e.Type)
            .ToListAsync(cancellationToken);

        var consumer = entities.FirstOrDefault(e => e.Id == consumerId);
        var supplier = entities.FirstOrDefault(e => e.Id == supplierId);

        var problem = ValidationComposer.Validate(
            EntityValidation.FromExists(consumer),
            EntityValidation.ToExists(supplier),
            EntityTypeValidation.FromIsOfType(consumer?.TypeId ?? Guid.Empty, [EntityTypeConstants.Organization.Id]),
            EntityTypeValidation.ToIsOfType(supplier?.TypeId ?? Guid.Empty, [EntityTypeConstants.Organization.Id])
        );

        if (problem is { })
        {
            return (problem, default);
        }

        return (true, (consumer, supplier));
    }

    private static bool IsOrganization(Entity entity)
        => entity.TypeId == EntityTypeConstants.Organization.Id;

    private async Task<List<Guid>> GetResourceIdsByScope(string scope, CancellationToken cancellationToken)
    {
        // Query resources by scope claim (this assumes resources have scope metadata stored)
        // For now, we'll use RefId matching - adjust based on actual schema
        var resources = await dbContext.Resources
            .Include(r => r.Type)
            .Where(r => r.Type.Name == MaskinportenSchemaTypeName)
            .Where(r => r.RefId.Contains(scope)) // Simplified - adjust based on actual scope storage
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        return resources;
    }

    #endregion
}
