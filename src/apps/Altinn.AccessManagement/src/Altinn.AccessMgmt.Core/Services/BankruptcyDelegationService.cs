using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Appsettings;
using Altinn.AccessMgmt.Core.Notifications;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Utils.Helper;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.Api.Contracts.AccessManagement.Enums;

using Altinn.Authorization.ProblemDetails;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace Altinn.AccessMgmt.Core.Services
{
    public class BankruptcyDelegationService(AppDbContext db, IConnectionService connectionService, IAssignmentService assignmentService, IOptions<CoreAppsettings> appsettings) : IBankruptcyDelegationService
    {
        private IEnumerable<ConstantDefinition<EntityType>> SupportedToTypes { get; } = [
            EntityTypeConstants.Person,
        ];

        private async Task<List<Assignment>> GetBankruptcyEstateAssignmentsForParty(Guid party, CancellationToken cancellationToken = default)
        {
            return await db.Assignments
                .AsNoTracking()
                .Where(a => a.ToId == party && a.RoleId == RoleConstants.EstateAdministrator)
                .Include(a => a.From)
                .ToListAsync(cancellationToken);
        }

        private async Task<List<Package>> GetBankruptcyEstateAssignmentPackagesForParty(CancellationToken cancellationToken = default)
        {
            return await db.RolePackages
                .AsNoTracking()
                .Where(rp => rp.RoleId == RoleConstants.EstateAdministrator)
                .Include(rp => rp.Package)
                .Select(rp => rp.Package)
                .ToListAsync(cancellationToken);
        }

        private Task<(Entity From, Entity To)> GetFromAndToEntities(Guid? fromId, Guid? toId, CancellationToken cancellationToken) =>
            ConnectionWriteValidation.GetFromAndToEntitiesAsync(db, fromId, toId, cancellationToken);

        private static ValidationProblemInstance ValidateWriteOpInput(Entity from, Entity to, ConnectionOptions options) =>
            ConnectionWriteValidation.ValidateWriteOpInput(from, to, options);

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateForParty(Guid party, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            List<Guid> estates = assignments.Select(a => a.FromId).ToList();

            var packages = (await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken)).Select(p => p.Id).ToList();

            var userAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.From)
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.RoleId == RoleConstants.Rightholder && packages.Contains(a.AssignmentPackage.PackageId) && estates.Contains(a.Assignment.FromId))
                .ToListAsync(cancellationToken);

            return userAssignments
                .Select(access =>
                    new BankruptcyEstateAssignmentsDto()
                    {
                        // Add mapping here
                        AssignmentId = access.Assignment.Id,
                        PackageId = access.AssignmentPackage.PackageId,
                        FromId = access.Assignment.FromId,
                        ToId = access.Assignment.ToId,
                        PackageCode = PackageConstants.TryGetById(access.AssignmentPackage.PackageId, out var result) ? result.Entity.Code : null,
                    }).ToList();
        }

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForUser(Guid party, Guid? user, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            List<Guid> estates = assignments.Select(a => a.FromId).ToList();

            var packages = (await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken)).Select(p => p.Id).ToList();

            var userAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.From)
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.ToId == user && a.Assignment.RoleId == RoleConstants.Rightholder && packages.Contains(a.AssignmentPackage.PackageId) && estates.Contains(a.Assignment.FromId))
                .ToListAsync(cancellationToken);

            return userAssignments
                .Select(access =>
                    new BankruptcyEstateAssignmentsDto()
                    {
                        // Add mapping here
                        AssignmentId = access.Assignment.Id,
                        PackageId = access.AssignmentPackage.PackageId,
                        FromId = access.Assignment.FromId,
                        ToId = access.Assignment.ToId,
                        PackageCode = PackageConstants.TryGetById(access.AssignmentPackage.PackageId, out var result) ? result.Entity.Code : null,
                    }).ToList();
        }

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForEstate(Guid party, Guid estate, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            var packages = (await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken)).Select(p => p.Id).ToList();

            var estateAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.From)
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.FromId == estate && a.Assignment.RoleId == RoleConstants.Rightholder && packages.Contains(a.AssignmentPackage.PackageId))
                .ToListAsync(cancellationToken);

            return estateAssignments
                .Select(access =>
                    new BankruptcyEstateAssignmentsDto()
                    {
                        // Add mapping here
                        AssignmentId = access.Assignment.Id,
                        PackageId = access.AssignmentPackage.PackageId,
                        FromId = access.Assignment.FromId,
                        ToId = access.Assignment.ToId,
                        PackageCode = PackageConstants.TryGetById(access.AssignmentPackage.PackageId, out var result) ? result.Entity.Code : null,
                    }).ToList();
        }

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> AddBankruptcyEstatePackagesToUser(Guid party, Guid user, Guid estate, List<string> packageCodes, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(estate, user, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsNoTracking()
                .Where(e => e.FromId == from.Id)
                .Where(e => e.ToId == to.Id)
                .Where(e => e.RoleId == RoleConstants.Rightholder)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment is null)
            {
                return Problems.AssignmentNotFound;
            }

            var allowedPackages = await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken);

            var packages = allowedPackages.Where(p => packageCodes.Contains(p.Code)).ToList();
            
            if (packages.Count != packageCodes.Count)
            {
                return Problems.PackageNotAvailableForDelegation;
            }

            List<AssignmentPackage> assignmentPackages = [];

            foreach (var package in packages)
            {
                var existingAssignmentPackage = await db.AssignmentPackages
                    .AsNoTracking()
                    .Where(ap => ap.AssignmentId == existingAssignment.Id && ap.PackageId == package.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                if (existingAssignmentPackage is null)
                {
                    var newAssignmentPackage = new AssignmentPackage
                    {
                        AssignmentId = existingAssignment.Id,
                        PackageId = package.Id
                    };
                    db.AssignmentPackages.Add(newAssignmentPackage);
                    assignmentPackages.Add(newAssignmentPackage);
                }
                else
                {
                    assignmentPackages.Add(existingAssignmentPackage);
                }
            }

            db.SaveChanges();

            return assignmentPackages
                .Select(ap =>
                    new BankruptcyEstateAssignmentsDto()
                    {
                        AssignmentId = existingAssignment.Id,
                        PackageId = ap.PackageId,
                        FromId = existingAssignment.FromId,
                        ToId = existingAssignment.ToId,
                        PackageCode = PackageConstants.TryGetById(ap.PackageId, out var result) ? result.Entity.Code : null,
                    }).ToList();            
        }

        /// <inheritdoc />
        public async Task<Result<int>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packageCodes, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(estate, user, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsNoTracking()
                .Where(e => e.FromId == from.Id)
                .Where(e => e.ToId == to.Id)
                .Where(e => e.RoleId == RoleConstants.Rightholder)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment is null)
            {
                return Problems.AssignmentNotFound;
            }

            var allowedPackages = await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken);

            var packages = allowedPackages.Where(p => packageCodes.Contains(p.Code)).ToList();

            if (packages.Count != packageCodes.Count)
            {
                return Problems.PackageNotAvailableForDelegation;
            }

            var assignmentPackagesToRemove = await db.AssignmentPackages
                .AsNoTracking()
                .Where(ap => ap.AssignmentId == existingAssignment.Id && packages.Select(p => p.Id).Contains(ap.PackageId))
                .ToListAsync(cancellationToken);

            db.AssignmentPackages.RemoveRange(assignmentPackagesToRemove);
            int removedCount = await db.SaveChangesAsync(cancellationToken);

            return removedCount;
        }

        /// <inheritdoc />
        public async Task<bool> CheckBankruptcyEstateConnection(Guid party, Guid estate, CancellationToken cancellationToken = default)
        {
            var estateAssignments = await db.Assignments
                .AsNoTracking()
                .Where(a => a.FromId == estate && a.ToId == party && a.RoleId == RoleConstants.EstateAdministrator)
                .ToListAsync(cancellationToken);

            if (estateAssignments.Count > 0)
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public async Task<Result<bool>> AddCreditor(Guid party, Guid estate, Guid creditor, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default)
        {
            var result = await connectionService.AddRightholder(estate, creditor, configureConnections, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem;
            }

            var assignmentId = result.Value.Id;

            var existingAssignmentPackage = await db.AssignmentPackages
                    .AsNoTracking()
                    .Where(ap => ap.AssignmentId == assignmentId && ap.PackageId == PackageConstants.BankruptcyEstateReadAccess.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignmentPackage is null)
            {
                var newAssignmentPackage = new AssignmentPackage
                {
                    AssignmentId = assignmentId,
                    PackageId = PackageConstants.BankruptcyEstateReadAccess.Entity.Id
                };

                db.AssignmentPackages.Add(newAssignmentPackage);
                db.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<Result<bool>> RevokeCreditor(Guid party, Guid estate, Guid creditor, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(estate, creditor, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsNoTracking()
                .Where(e => e.FromId == from.Id)
                .Where(e => e.ToId == to.Id)
                .Where(e => e.RoleId == RoleConstants.Rightholder)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment is null)
            {
                return false;
            }

            var assignmentPackagesToRemove = await db.AssignmentPackages
                .AsNoTracking()
                .Where(ap => ap.AssignmentId == existingAssignment.Id && ap.PackageId == PackageConstants.BankruptcyEstateReadAccess.Id)
                .ToListAsync(cancellationToken);

            db.AssignmentPackages.RemoveRange(assignmentPackagesToRemove);
            int removedCount = await db.SaveChangesAsync(cancellationToken);

            var result = await assignmentService.DeleteAssignment(existingAssignment.Id, false, null, cancellationToken);
            
            return removedCount > 0;
        }

        /// <inheritdoc />
        public async Task<Result<List<CompactEntityDto>>> GetCreditors(Guid party, Guid estate, CancellationToken cancellationToken = default)
        {
            var estateAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.To)
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.FromId == estate && a.Assignment.RoleId == RoleConstants.Rightholder && a.AssignmentPackage.PackageId == PackageConstants.BankruptcyEstateReadAccess.Id)
                .ToListAsync(cancellationToken);

            List<CompactEntityDto> creditors = new List<CompactEntityDto>();

            foreach (var access in estateAssignments)
            {
                creditors.Add(DtoMapper.Convert(access.Assignment.To));
            }
            
            return creditors;
        }

        /// <inheritdoc/>
        public async Task<Result<AssignmentDto>> AddAgent(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(party, user, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            ValidationErrorBuilder errorBuilder = default;

            var existingAssignment = await db.Assignments.AsNoTracking().Where(p => p.FromId == party && p.ToId == user && p.RoleId == RoleConstants.Agent).FirstOrDefaultAsync(cancellationToken);
            if (existingAssignment is { })
            {
                return DtoMapper.Convert(existingAssignment);
            }

            var entity = await db.Entities.FirstOrDefaultAsync(e => e.Id == user, cancellationToken);
            if (entity is null)
            {
                return Problems.EntityTypeNotFound;
            }

            if (!SupportedToTypes.Any(e => e.Id == entity.TypeId))
            {
                var supportedToTypeNames = string.Join(", ", SupportedToTypes.Select(t => t.Entity.Name));
                errorBuilder.Add(
                    ValidationErrors.DisallowedEntityType,
                    $"$QUERY/user",
                    [new($"{entity.TypeId}", $"Entity type is not supported as an agent. Supported types: <{supportedToTypeNames}>.")]
                );
            }            

            if (errorBuilder.TryBuild(out problem))
            {
                return problem;
            }

            var assignment = new Assignment
            {
                FromId = party,
                ToId = user,
                RoleId = RoleConstants.Agent,
            };

            db.Assignments.Add(assignment);
            await AgentAddedNotification.Upsert(
                db,
                party,
                user,
                appsettings?.Value?.Notifications?.AgentAddedNotifyInSeconds ?? AgentAddedNotification.DefaultNotifyInSeconds,
                cancellationToken
            );
            await db.SaveChangesAsync(cancellationToken);

            return DtoMapper.Convert(assignment);
        }

        /// <inheritdoc/>
        public async Task<ValidationProblemInstance?> RevokeAgent(Guid party, Guid user, bool cascade, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(party, user, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsTracking()
                .Where(p => p.FromId == party && p.ToId == user && p.RoleId == RoleConstants.Agent)
                .FirstOrDefaultAsync(cancellationToken);

            if (!cascade)
            {
                ValidationErrorBuilder errorBuilder = await CascadingRevokeHelper.CheckCascadingDependenciesAgentAssignment(db, existingAssignment, cascade, cancellationToken);

                if (errorBuilder.TryBuild(out problem))
                {
                    return problem;
                }
            }

            await AgentRemovedNotification.Upsert(
                db,
                party,
                user,
                appsettings?.Value?.Notifications?.AgentRemovedNotifyInSeconds ?? AgentRemovedNotification.DefaultNotifyInSeconds,
                cancellationToken
            );

            db.Assignments.Remove(existingAssignment);
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        /// <inheritdoc/>
        public async Task<Result<List<BankruptcyEntityDto>>> GetAgentAdminInformation(Guid party, CancellationToken cancellationToken)
        {
            var agentAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.To)
                .Where(a => a.FromId == party && a.RoleId == RoleConstants.Agent)
                .ToListAsync(cancellationToken);

            var adminAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.To)
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.FromId == party && a.Assignment.RoleId == RoleConstants.Rightholder && a.AssignmentPackage.PackageId == PackageConstants.KonkursboAdministrator.Id)
                .ToListAsync(cancellationToken);

            var agentUsers = agentAssignments.Select(a => DtoMapper.Convert(a.To)).ToList();
            var adminUsers = adminAssignments.Select(a => DtoMapper.Convert(a.Assignment.To)).ToList();
            
            var result = agentUsers
                .GroupJoin(adminUsers, l1 => l1, l2 => l2, (l1, l2Group) => new { Item = l1, Match = l2Group.Any() })
                .Select(x => new BankruptcyEntityDto(x.Item, x.Match ? BankruptcyEstatePermissions.UserAndAdmin : BankruptcyEstatePermissions.User))
                .Union(
                    adminUsers
                        .GroupJoin(agentUsers, l2 => l2, l1 => l1, (l2, l1Group) => new { Item = l2, Match = l1Group.Any() })
                        .Select(x => new BankruptcyEntityDto(x.Item, x.Match ? BankruptcyEstatePermissions.UserAndAdmin : BankruptcyEstatePermissions.Admin))
                )
                .Distinct()
                .ToList();

            return result;
        }

        public async Task<Result<AssignmentDto>> AddAdministrator(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken)
        {
            var result = await connectionService.AddRightholder(party, user, configureConnections, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem;
            }

            var assignmentId = result.Value.Id;

            var existingAssignmentPackage = await db.AssignmentPackages
                    .AsNoTracking()
                    .Where(ap => ap.AssignmentId == assignmentId && ap.PackageId == PackageConstants.KonkursboAdministrator.Id)
                    .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignmentPackage is null)
            {
                var newAssignmentPackage = new AssignmentPackage
                {
                    AssignmentId = assignmentId,
                    PackageId = PackageConstants.BankruptcyEstateReadAccess.Entity.Id
                };

                db.AssignmentPackages.Add(newAssignmentPackage);
                db.SaveChanges();
            }

            return result.Value;
        }

        public async Task<Result<bool>> RevokeAdministrator(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(party, user, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsNoTracking()
                .Where(e => e.FromId == from.Id)
                .Where(e => e.ToId == to.Id)
                .Where(e => e.RoleId == RoleConstants.Rightholder)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment is null)
            {
                return false;
            }

            var assignmentPackagesToRemove = await db.AssignmentPackages
                .AsNoTracking()
                .Where(ap => ap.AssignmentId == existingAssignment.Id && ap.PackageId == PackageConstants.KonkursboAdministrator.Id)
                .ToListAsync(cancellationToken);

            db.AssignmentPackages.RemoveRange(assignmentPackagesToRemove);
            int removedCount = await db.SaveChangesAsync(cancellationToken);

            var result = await assignmentService.DeleteAssignment(existingAssignment.Id, false, null, cancellationToken);

            return removedCount > 0;
        }
    }

    /// <summary>
    /// Service for managing client delegations and delegation of access packageCodes.
    /// </summary>
    public interface IBankruptcyDelegationService
    {
        /// <summary>
        /// Gets the bankruptcy estate assignments for a given party.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of bankruptcy estate assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateForParty(Guid party, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the clients for a given party, filtered by roles and packageCodes.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">Optional user identifier to filter the clients.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of clients or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForUser(Guid party, Guid? user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the bankruptcy estate assignments for a given estate.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The estate identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of bankruptcy estate assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForEstate(Guid party, Guid estate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds packageCodes to a user for a specific bankruptcy estate.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="packages">The list of packageCodes to add.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of updated assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> AddBankruptcyEstatePackagesToUser(Guid party, Guid user, Guid estate, List<string> packages, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes packageCodes from a user for a specific bankruptcy estate.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="packages">The list of packageCodes to revoke.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Removed package count.</returns>
        Task<Result<int>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packages, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a given estate is connected to the party
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The estate identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the estate is connected to the party, or a problem detail if an error occurs.</returns>
        Task<bool> CheckBankruptcyEstateConnection(Guid party, Guid estate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a rettighetshaver assignment and adds the package BankruptcyEstateReadAccess to the assignment for a specific bankruptcy estate.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="creditor">The user identifier.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. true if read access is added and false if it alredy exists</returns>
        Task<Result<bool>> AddCreditor(Guid party, Guid estate, Guid creditor, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the package BankruptcyEstateReadAccess from the assignment between the creditor and the bankruptcy estate if the rettighetshaver assignment holds no more content the assignment is also removed.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="creditor">The user identifier.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. true if read access is revoked and false if it alredy was revoked</returns>
        Task<Result<bool>> RevokeCreditor(Guid party, Guid estate, Guid creditor, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of creditors for a specific bankruptcy estate. 
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. List of creditors if successful.</returns>
        Task<Result<List<CompactEntityDto>>> GetCreditors(Guid party, Guid estate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds an agent relationship between two entities.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The entity the Agent relationship is defined for</param>
        /// <param name="user">The entity the agent relationship is given to</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. The assignment details if successful.</returns>
        Task<Result<AssignmentDto>> AddAgent(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes an agent relationship between two entities.
        /// </summary>
        /// <param name="party">The entity the Agent relationship is defined for</param>
        /// <param name="user">The entity the agent relationship is given to</param>
        /// <param name="cascade">If true the revoke is performed even when there are active dependencies else it will fail if there are active dependencies</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>error or nothing</returns>
        Task<ValidationProblemInstance?> RevokeAgent(Guid party, Guid user, bool cascade, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of agents/administrators for a specific party.
        /// 
        /// It is the callers responsibility to check if the party has access to the estate before calling this method.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. List of agents/administrators if successful.</returns>
        Task<Result<List<BankruptcyEntityDto>>> GetAgentAdminInformation(Guid party, CancellationToken cancellationToken);

        /// <summary>
        /// Adds rightholder role to a user and assign the boadministrator package for a specific party.
        /// </summary>
        /// <param name="party">The entity the rightholder relationship is defined for</param>
        /// <param name="user">he entity the agent relationship is given to</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. The assignment details if successful.</returns>
        Task<Result<AssignmentDto>> AddAdministrator(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken);

        /// <summary>
        /// Revokes rightholder role from a user and removes the boadministrator package for a specific party.
        /// </summary>
        /// <param name="party">The entity the rightholder relationship is defined for</param>
        /// <param name="user">The entity the agent relationship is given to</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>error or nothing</returns>
        Task<Result<bool>> RevokeAdministrator(Guid party, Guid user, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken);
    }
}
