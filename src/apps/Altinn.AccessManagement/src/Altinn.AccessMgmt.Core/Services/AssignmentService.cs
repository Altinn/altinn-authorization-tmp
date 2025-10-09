﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Utils.Models;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Extensions;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.AccessMgmt.Core.Services;

/// <inheritdoc/>
public class AssignmentService(AppDbContext db) : IAssignmentService
{
    /// <inheritdoc/>
    public async Task<IEnumerable<ClientDto>> GetClients(Guid toId, string[] roles, string[] packages, CancellationToken cancellationToken = default)
    {
        // Fetch role metadata
        var roleResult = QueryWrapper.WrapQueryResponse(await db.Roles.AsNoTracking().Where(t => roles.Contains(t.Code)).ToListAsync(cancellationToken));

        if (roleResult == null || !roleResult.Any() || roleResult.Count() != roles.Length)
        {
            throw new ArgumentException($"Filter: {nameof(roles)}, provided contains one or more role identifiers which cannot be found.");
        }

        var filterRoleIds = roleResult.Select(r => r.Id).ToList();

        // Fetch role-package metadata
        var rolePackageResult = QueryWrapper.WrapQueryResponse(await db.RolePackages.AsNoTracking().Where(t => filterRoleIds.Contains(t.RoleId)).ToListAsync(cancellationToken));

        // Fetch package metadata
        var packageResult = QueryWrapper.WrapQueryResponse(await db.Packages.AsNoTracking().ToListAsync(cancellationToken));

        if (!packages.All(p => packageResult.Select(pr => pr.Urn).Contains($"urn:altinn:accesspackage:{p}")))
        {
            throw new ArgumentException($"Filter: {nameof(packages)}, provided contains one or more package identifiers which cannot be found.");
        }

        // Fetch client assignments
        var clientAssignmentResult = 
            await db.Assignments.AsNoTracking()
                .Include(t => t.From)
                .Where(t => t.ToId == toId && filterRoleIds.Contains(t.RoleId))
            .ToListAsync(cancellationToken);

        // Discard non-organization clients (for now). To be opened up for private individuals in the future.
        var clients = clientAssignmentResult.Where(c => c.From.TypeId == EntityTypeConstants.Organisation.Entity.Id);

        // Fetch assignment packages
        QueryResponse<AssignmentPackage> assignmentPackageResult = null;
        if (roles.Contains(RoleConstants.Rightholder.Entity.Code))
        {
            var rettighetshaverClients = clients.Where(c => c.RoleId == roleResult.First(r => r.Code == RoleConstants.Rightholder.Entity.Code).Id);
            if (rettighetshaverClients.Any())
            {
                assignmentPackageResult = QueryWrapper.WrapQueryResponse(await db.AssignmentPackages.AsNoTracking().Where(t => rettighetshaverClients.Select(p => p.Id).Contains(t.AssignmentId)).ToListAsync(cancellationToken));
            }
        }

        return await GetFilteredClientsFromAssignments(clients, assignmentPackageResult, roleResult, packageResult, rolePackageResult, packages, cancellationToken);
    }

    private async Task<List<ClientDto>> GetFilteredClientsFromAssignments(IEnumerable<Assignment> assignments, IEnumerable<AssignmentPackage> assignmentPackages, QueryResponse<Role> roles, QueryResponse<Package> packages, QueryResponse<RolePackage> rolePackages, string[] filterPackages, CancellationToken cancellationToken)
    {
        Dictionary<Guid, ClientDto> clients = new();

        // Fetch Entity metadata        
        var entityVariants = await db.EntityVariants.AsNoTracking().ToListAsync(cancellationToken);

        foreach (var assignment in assignments)
        {
            var roleName = roles.First(r => r.Id == assignment.RoleId).Code;
            var assignmentPackageIds = assignmentPackages != null ? assignmentPackages.Where(ap => ap.AssignmentId == assignment.Id).Select(ap => ap.PackageId) : [];
            var assignmentPackageNames = assignmentPackageIds.Any() ? assignmentPackageIds.Select(ap => packages.First(p => p.Id == ap).Urn.Split(":").Last()).ToArray() : [];
            var rolePackageIds = rolePackages.Where(rp => rp.RoleId == assignment.RoleId && (!rp.EntityVariantId.HasValue || rp.EntityVariantId == assignment.From.VariantId)).Select(rp => rp.PackageId);
            var rolePackageNames = rolePackageIds.Select(rp => packages.First(p => p.Id == rp).Urn.Split(":").Last()).ToArray();

            // Skip client if connection provides neither assignment-packages or role-packages
            if (assignmentPackageNames.Length == 0 && rolePackageNames.Length == 0)
            {
                continue;
            }

            // Add client to dictionary if not already present
            if (!clients.TryGetValue(assignment.FromId, out ClientDto client))
            {
                client = new ClientDto()
                {
                    Party = new ClientDto.ClientParty
                    {
                        Id = assignment.FromId,
                        Name = assignment.From.Name,
                        OrganizationNumber = assignment.From.RefId,
                        UnitType = entityVariants.FirstOrDefault(ev => ev.Id == assignment.From.VariantId)?.Name
                    }
                };

                clients.Add(assignment.FromId, client);
            }

            // Add packages client has been assigned
            if (assignmentPackageNames.Length > 0)
            {
                client.Access.Add(new ClientDto.ClientRoleAccessPackages
                {
                    Role = roleName,
                    Packages = assignmentPackageNames
                });
            }

            // Add packages client has through role
            if (rolePackageNames.Length > 0)
            {
                client.Access.Add(new ClientDto.ClientRoleAccessPackages
                {
                    Role = roleName,
                    Packages = rolePackageNames
                });
            }
        }

        // Return only clients having all required filterpackages
        List<ClientDto> result = new();
        foreach (var client in clients.Keys)
        {
            var allClientPackages = clients[client].Access.SelectMany(rp => rp.Packages).Distinct();
            if (filterPackages.All(allClientPackages.Contains))
            {
                result.Add(clients[client]);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid id, CancellationToken cancellationToken = default)
    {
        return await db.Assignments.AsNoTracking()
            .Include(t => t.Role)
            .SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var result = await db.Assignments.AsNoTracking()
            .Where(t => t.FromId == fromId && t.ToId == toId && t.RoleId == roleId)
            .ToListAsync(cancellationToken);
        if (result == null || !result.Any())
        {
            return null;
        }

        return result.First();
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetAssignment(Guid fromId, Guid toId, string roleCode, CancellationToken cancellationToken = default)
    {
        var roleResult = await db.Roles.AsNoTracking()
            .Where(t => t.Code == roleCode)
            .ToListAsync(cancellationToken);

        if (roleResult == null || !roleResult.Any())
        {
            return null;
        }

        return await GetAssignment(fromId, toId, roleResult.First().Id);
    }

    /// <inheritdoc/>
    public async Task<bool> AddPackageToAssignment(Guid userId, Guid assignmentId, Guid packageId, CancellationToken cancellationToken = default)
    {
        /*
        [X] Check if user is TS
        [X] Check if user assignment.roles has packages
        [X] Check if user assignment.assignmentpackages has package
        [?] Check if users has packages delegated?

        [ ] Check if package can be delegated
        */

        /* TODO: Future Sjekk om bruker er Tilgangsstyrer */
        var user = await db.Entities.AsNoTracking().SingleAsync(t => t.Id == userId, cancellationToken);
        var assignment = await db.Assignments.SingleAsync(t => t.Id == assignmentId, cancellationToken);
        var package = await db.Packages.SingleAsync(t => t.Id == packageId, cancellationToken);

        var userAssignments = await db.Assignments.AsNoTracking()
            .Where(t => t.FromId == assignment.FromId && t.ToId == userId)
            .ToListAsync(cancellationToken);

        bool hasPackage = false;

        var assignmentMatches = await db.AssignmentPackages.AsNoTracking()
            .Where(t => t.PackageId == packageId && userAssignments.Select(t => t.Id)
            .Contains(t.AssignmentId))
            .ToListAsync(cancellationToken);

        if (assignmentMatches != null && assignmentMatches.Any())
        {
            hasPackage = true;
        }

        if (!hasPackage)
        {
            var roleMatches = await db.RolePackages.AsNoTracking()
                .Where(t => t.PackageId == packageId && userAssignments.Select(t => t.RoleId)
                .Distinct()
                .Contains(t.RoleId))
                .ToListAsync(cancellationToken);

            if (roleMatches != null && roleMatches.Any())
            {
                hasPackage = true;
            }
        }

        if (!hasPackage)
        {
            throw new Exception(string.Format("User '{0}' does not have package '{1}'", user.Name, package.Name));
        }

        await db.AssignmentPackages.AddAsync(
            new AssignmentPackage()
            {
                AssignmentId = assignmentId,
                PackageId = packageId
            }, 
            cancellationToken
            );
        
        var result = await db.SaveChangesAsync(cancellationToken);
        if (result == 0)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public Task<bool> AddResourceToAssignment(Guid userId, Guid assignmentId, Guid resourceId, CancellationToken cancellationToken = default)
    {
        /*
        [ ] Check if user is TS
        [ ] Check if resource can be delegated
        [ ] Check if user assignment.assignmentpackages has resources
        [ ] Check if user assignment.roles has packages
        [ ] Check if users has packages delegated?
        */

        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task<ProblemInstance> DeleteAssignment(Guid fromEntityId, Guid toEntityId, string roleCode, bool cascade = false, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;

        var fromEntity = await db.Entities.SingleAsync(t => t.Id == fromEntityId, cancellationToken);
        var toEntity = await db.Entities.SingleAsync(t => t.Id == toEntityId, cancellationToken);
        ValidatePartyIsNotNull(fromEntityId, fromEntity, ref errors, "$QUERY/party");
        ValidatePartyIsNotNull(toEntityId, toEntity, ref errors, "$QUERY/to");

        var roleResult = await db.Roles.AsNoTracking().Where(t => t.Code == roleCode).ToListAsync(cancellationToken);
        if (roleResult == null || !roleResult.Any() || roleResult.Count > 1)
        {
            Unreachable();
        }

        var role = roleResult.First();
        if (!role.IsAssignable)
        {
            errors.Add(ValidationErrors.UnableToRevokeRoleAssignment, "$QUERY/roleCode", [new("role", role.Code)]);
        }
        
        var existingAssignment = await GetAssignment(fromEntityId, toEntityId, role.Id, cancellationToken: cancellationToken);
        if (existingAssignment == null)
        {
            return null;
        }
        else
        {
            if (!cascade)
            {
                var packages = await db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == existingAssignment.Id).ToListAsync(cancellationToken);
                if (packages != null && packages.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("packages", string.Join(",", packages.Select(p => p.Id.ToString())))]);
                }

                var delegationsFromAssingment = await db.Delegations.AsNoTracking().Where(t => t.FromId == existingAssignment.Id).ToListAsync(cancellationToken);
                if (delegationsFromAssingment != null && delegationsFromAssingment.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("delegations", string.Join(",", delegationsFromAssingment.Select(p => p.Id.ToString())))]);
                }

                var delegationsToAssignment = await db.Delegations.AsNoTracking().Where(t => t.ToId == existingAssignment.Id).ToListAsync(cancellationToken);
                if (delegationsToAssignment != null && delegationsToAssignment.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("delegations", string.Join(",", delegationsToAssignment.Select(p => p.Id.ToString())))]);
                }
            }
        }

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        var assignment = await db.Assignments.SingleAsync(t => t.Id == existingAssignment.Id, cancellationToken);
        db.Assignments.Remove(assignment);
        var result = await db.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            Unreachable();
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<ProblemInstance> DeleteAssignment(Guid assignmentId, bool cascade = false, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;

        var existingAssignment = await GetAssignment(assignmentId, cancellationToken: cancellationToken);
        if (existingAssignment == null)
        {
            return null;
        }
        else
        {
            if (!existingAssignment.Role.IsAssignable)
            {
                errors.Add(ValidationErrors.UnableToRevokeRoleAssignment, "$QUERY/assignmentId", [new("role", existingAssignment.Role.Code)]);
            }

            if (!cascade)
            {
                var packages = await db.AssignmentPackages.AsNoTracking()
                    .Where(t => t.AssignmentId == existingAssignment.Id)
                    .ToListAsync(cancellationToken);

                if (packages != null && packages.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("packages", string.Join(",", packages.Select(p => p.Id.ToString())))]);
                }

                var delegationsFromAssingment = await db.Delegations.AsNoTracking()
                    .Where(t => t.FromId == existingAssignment.Id)
                    .ToListAsync(cancellationToken);
                if (delegationsFromAssingment != null && delegationsFromAssingment.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("delegations", string.Join(",", delegationsFromAssingment.Select(p => p.Id.ToString())))]);
                }

                var delegationsToAssignment = await db.Delegations.AsNoTracking()
                    .Where(t => t.ToId == existingAssignment.Id)
                    .ToListAsync(cancellationToken);
                if (delegationsToAssignment != null && delegationsToAssignment.Any())
                {
                    errors.Add(ValidationErrors.AssignmentIsActiveInOneOrMoreDelegations, "$QUERY/cascade", [new("delegations", string.Join(",", delegationsFromAssingment.Select(p => p.Id.ToString())))]);
                }
            }
        }

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        db.Assignments.Remove(existingAssignment);
        var result = await db.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            Unreachable();
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<Result<Assignment>> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, string roleCode, CancellationToken cancellationToken = default)
    {
        ValidationErrorBuilder errors = default;

        var fromEntity = await db.Entities.SingleAsync(t => t.Id == fromEntityId, cancellationToken);
        var toEntity = await db.Entities.SingleAsync(t => t.Id == toEntityId, cancellationToken);
        ValidatePartyIsNotNull(fromEntityId, fromEntity, ref errors, "$QUERY/party");
        ValidatePartyIsOrg(fromEntity, ref errors, "$QUERY/party");
        ValidatePartyIsNotNull(toEntityId, toEntity, ref errors, "$QUERY/to");
        ValidatePartyIsOrg(toEntity, ref errors, "$QUERY/to");

        var roleResult = await db.Roles.AsNoTracking().Where(t => t.Code == roleCode).ToListAsync(cancellationToken);
        if (roleResult == null || !roleResult.Any())
        {
            Unreachable();
        }

        var roleId = roleResult.First().Id;
        var existingAssignment = await GetAssignment(fromEntityId, toEntityId, roleId, cancellationToken: cancellationToken);
        if (existingAssignment != null)
        {
            return existingAssignment;
        }

        if (errors.TryBuild(out var errorResult))
        {
            return errorResult;
        }

        var assignment = new Assignment
        {
            FromId = fromEntityId,
            ToId = toEntityId,
            RoleId = roleId,
        };

        await db.Assignments.AddAsync(assignment, cancellationToken);
        var result = await db.SaveChangesAsync(cancellationToken);

        if (result == 0)
        {
            Unreachable();
        }

        return assignment;
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetOrCreateAssignmentInternal(Guid fromEntityId, Guid toEntityId, string roleCode, CancellationToken cancellationToken = default)
    {
        var roleResult = await db.Roles.AsNoTracking().Where(t => t.Code == roleCode).ToListAsync(cancellationToken);
        if (roleResult == null || !roleResult.Any())
        {
            throw new Exception(string.Format("Role '{0}' not found", roleCode));
        }

        return await GetOrCreateAssignment(fromEntityId, toEntityId, roleResult.First().Id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Assignment> GetOrCreateAssignment(Guid fromEntityId, Guid toEntityId, Guid roleId, AuditValues audit = null, CancellationToken cancellationToken = default)
    {
        var assignment = await GetAssignment(fromEntityId, toEntityId, roleId, cancellationToken: cancellationToken);
        if (assignment != null)
        {
            return assignment;
        }

        var role = await db.Roles.AsNoTracking().SingleAsync(t => t.Id == roleId, cancellationToken);
        if (role == null)
        {
            throw new Exception(string.Format("Role '{0}' not found", roleId));
        }

        /*
        var inheritedAssignments = await GetInheritedAssignment(fromEntityId, toEntityId, role.Id, cancellationToken: cancellationToken);
        if (inheritedAssignments != null && inheritedAssignments.Any())
        {
            if (inheritedAssignments.Count() == 1)
            {
                throw new Exception(string.Format("An inheirited assignment exists From:'{0}.FromName' Via:'{0}.ViaName' To:'{}.ToName'. Use Force = true to create anyway.", inheritedAssignments.First()));
            }

            throw new Exception(string.Format("Multiple inheirited assignment exists. Use Force = true to create anyway."));
        }
        */

        assignment = new Assignment()
        {
            FromId = fromEntityId,
            ToId = toEntityId,
            RoleId = role.Id,
        };
        
        await db.Assignments.AddAsync(assignment, cancellationToken);
        int result;

        if (audit == null)
        {
            result = await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            result = await db.SaveChangesAsync(audit, cancellationToken);
        }

        if (result == 0)
        {
            Unreachable();
        }

        return assignment;
    }

    /*
    /// <inheritdoc/>
    public async Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, Guid roleId, CancellationToken cancellationToken = default)
    {
        var filter = inheritedAssignmentRepository.CreateFilterBuilder();
        filter.Equal(t => t.FromId, fromId);
        filter.Equal(t => t.ToId, toId);
        filter.Equal(t => t.RoleId, roleId);

        return await inheritedAssignmentRepository.Get(filter, cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<InheritedAssignment>> GetInheritedAssignment(Guid fromId, Guid toId, string roleCode, CancellationToken cancellationToken = default)
    {
        var roleResult = await roleRepository.Get(t => t.Code, roleCode, cancellationToken: cancellationToken);
        if (roleResult == null || !roleResult.Any())
        {
            throw new Exception(string.Format("Role not found '{0}'", roleCode));
        }

        var roleId = roleResult.First().Id;
        return await GetInheritedAssignment(fromId, toId, roleId, cancellationToken: cancellationToken);
    }
    */

    public async Task<IEnumerable<Resource>> GetAssignmentResources(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        return await db.AssignmentResources.AsNoTracking().Where(t => t.AssignmentId == assignmentId).Include(t => t.Resource).Select(t => t.Resource).ToListAsync();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AssignmentOrRolePackageAccess>> GetPackagesForAssignment(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        List<AssignmentOrRolePackageAccess> result = new();

        // Get Assignment
        var assignment = await db.Assignments.AsNoTracking().SingleAsync(t => t.Id == assignmentId, cancellationToken);

        // Add validation (@Isnes)
        if (assignment == null)
        {
            return result;
        }

        // Get AssignmentPackages
        QueryResponse<AssignmentPackage> assignmentPackageResult = null;
        assignmentPackageResult = QueryWrapper.WrapQueryResponse(await db.AssignmentPackages.AsNoTracking().Where(t => t.AssignmentId == assignment.Id).ToListAsync(cancellationToken));

        foreach (var assignmentPackage in assignmentPackageResult)
        {
            result.Add(new AssignmentOrRolePackageAccess { AssignmentId = assignment.Id, RoleId = assignment.RoleId, PackageId = assignmentPackage.PackageId, AssignmentPackageId = assignmentPackage.Id });
        }

        // Get RolePackages
        var rolePackageResult = await db.RolePackages.AsNoTracking().Where(t => t.RoleId == assignment.RoleId).ToListAsync(cancellationToken);

        foreach (var rolePackage in rolePackageResult)
        {
            result.Add(new AssignmentOrRolePackageAccess { AssignmentId = assignment.Id, RoleId = assignment.RoleId, PackageId = rolePackage.PackageId, RolePackageId = rolePackage.Id });
        }

        return result;
    }

    private static void ValidatePartyIsNotNull(Guid id, Entity entity, ref ValidationErrorBuilder errors, string param)
    {
        if (entity is null)
        {
            errors.Add(ValidationErrors.EntityNotExists, param, [new("partyId", id.ToString())]);
        }
    }

    private static void ValidatePartyIsOrg(Entity entity, ref ValidationErrorBuilder errors, string param)
    {
        if (entity is not null && !entity.Type.Name.Equals("Organisasjon", StringComparison.InvariantCultureIgnoreCase))
        {
            errors.Add(ValidationErrors.InvalidQueryParameter, param, [new("partyId", $"expected party of type 'Organisasjon' got '{entity.Type.Name}'.")]);
        }
    }

    [DoesNotReturn]
    private static void Unreachable()
    {
        throw new UnreachableException();
    }
}
