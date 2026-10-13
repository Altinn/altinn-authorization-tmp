using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;

using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Altinn.AccessMgmt.Core.Services
{
    public class BankruptcyEstateDelegationService(
        AppDbContext db) : IBankruptcyEstateDelegationService
    {
        private const int MaxSaveAttempts = 3;

        private Task<(Entity From, Entity To)> GetFromAndToEntities(Guid? fromId, Guid? toId, CancellationToken cancellationToken) =>
           ConnectionWriteValidation.GetFromAndToEntitiesAsync(db, fromId, toId, cancellationToken);

        private static ValidationProblemInstance ValidateWriteOpInput(Entity from, Entity to, ConnectionOptions options) =>
            ConnectionWriteValidation.ValidateWriteOpInput(from, to, options);

        private async Task<List<Entity>> GetBankruptcyEstatesInternal(Guid party, CancellationToken cancellationToken = default)
        {
            var assignments = await db.Assignments
                .AsNoTracking()
                .Where(a => a.ToId == party && a.RoleId == RoleConstants.EstateAdministrator)
                .Include(a => a.From)
                .ToListAsync(cancellationToken);

            return assignments.Select(a => a.From).ToList();
        }

        /// <inheritdoc />
        public async Task<bool> HasBankruptcyEstates(Guid partyUuid, CancellationToken cancellationToken = default)
        {
            return await db.Assignments
                .AsNoTracking()
                .AnyAsync(a => a.ToId == partyUuid && a.RoleId == RoleConstants.EstateAdministrator, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Result<List<CompactEntityDto>>> GetBankruptcyEstates(Guid party, CancellationToken cancellationToken = default)
        {
            var estates = await GetBankruptcyEstatesInternal(party, cancellationToken);
            return estates.Select(a => DtoMapper.Convert(a)).ToList();
        }

        /// <inheritdoc />
        public async Task<Result<List<CompactEntityDto>>> GetBankruptcyEstateAdministrators(Guid partyUuid, CancellationToken cancellationToken = default)
        {
            var adminAssignments = await db.Assignments
                .AsNoTracking()
                .Include(a => a.To)
                .Where(a => a.FromId == partyUuid && a.RoleId == RoleConstants.BankruptcyEstateAdministrator)
                .ToListAsync(cancellationToken);

            var adminUsers = adminAssignments.Select(a => DtoMapper.Convert(a.To)).ToList();

            return adminUsers;
        }

        /// <inheritdoc />
        public async Task<Result<AssignmentDto>> AddBankruptcyEstateAdministrator(Guid partyUuid, Guid toUuid, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);

            (Entity from, Entity to) = await GetFromAndToEntities(partyUuid, toUuid, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            Assignment adminAssignment = null;
            await SaveWithRetryOnUniqueViolation(
                async () =>
                {
                    adminAssignment = await db.Assignments.AsNoTracking().Where(p => p.FromId == partyUuid && p.ToId == toUuid && p.RoleId == RoleConstants.BankruptcyEstateAdministrator).FirstOrDefaultAsync(cancellationToken);
                    if (adminAssignment is null)
                    {
                        adminAssignment = new Assignment
                        {
                            FromId = partyUuid,
                            ToId = toUuid,
                            RoleId = RoleConstants.BankruptcyEstateAdministrator,
                        };

                        db.Assignments.Add(adminAssignment);
                    }

                    var estateIds = await GetBankruptcyEstateIds(partyUuid, cancellationToken);
                    await GrantEstateAccess(estateIds, [toUuid], cancellationToken);
                },
                cancellationToken);

            return DtoMapper.Convert(adminAssignment);
        }

        /// <inheritdoc />
        public async Task<int> GrantEstateAccessToAdministrators(Guid estateId, Guid estateAdministratorPartyId, CancellationToken cancellationToken = default)
        {
            return await SaveWithRetryOnUniqueViolation(
                async () =>
                {
                    var adminIds = await GetBankruptcyEstateAdministratorIds(estateAdministratorPartyId, cancellationToken);
                    await GrantEstateAccess([estateId], adminIds, cancellationToken);
                },
                cancellationToken);
        }

        /// <summary>
        /// Stages the changes and saves them. The staging reads existing rows and inserts the missing ones, which
        /// races with a concurrent request doing the same: both can see a row as missing, and the unique index on
        /// (FromId, ToId, RoleId) or (AssignmentId, PackageId) then rejects the second insert. On a unique violation
        /// the pending changes are discarded and staged again, so the rows the other request committed are found
        /// and reused instead of inserted.
        /// </summary>
        private async Task<int> SaveWithRetryOnUniqueViolation(Func<Task> stageChanges, CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                await stageChanges();
                try
                {
                    return await db.SaveChangesAsync(cancellationToken);
                }
                catch (DbUpdateException ex) when (attempt < MaxSaveAttempts && ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
                {
                    db.ChangeTracker.Clear();
                }
            }
        }

        /// <inheritdoc />
        public async Task<int> RevokeEstateAccessFromAdministrators(Guid estateId, Guid estateAdministratorPartyId, CancellationToken cancellationToken = default)
        {
            var adminIds = await GetBankruptcyEstateAdministratorIds(estateAdministratorPartyId, cancellationToken);
            await RevokeEstateAccess([estateId], adminIds, cancellationToken);

            return await db.SaveChangesAsync(cancellationToken);
        }

        private async Task<List<Guid>> GetBankruptcyEstateIds(Guid party, CancellationToken cancellationToken)
        {
            return await db.Assignments
                .AsNoTracking()
                .Where(a => a.ToId == party && a.RoleId == RoleConstants.EstateAdministrator)
                .Select(a => a.FromId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        private async Task<List<Guid>> GetBankruptcyEstateAdministratorIds(Guid estateAdministratorPartyId, CancellationToken cancellationToken)
        {
            return await db.Assignments
                .AsNoTracking()
                .Where(a => a.FromId == estateAdministratorPartyId && a.RoleId == RoleConstants.BankruptcyEstateAdministrator)
                .Select(a => a.ToId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Adds the Rightholder assignment from every estate to every administrator, with the packages
        /// MainAdministrator, BankruptcyEstateReadAccess and BankruptcyEstateWriteAccess, where missing.
        /// Existing assignments and packages are loaded in two queries regardless of the number of estates.
        /// Changes are added to the context and must be saved by the caller.
        /// </summary>
        private async Task GrantEstateAccess(List<Guid> estateIds, List<Guid> adminIds, CancellationToken cancellationToken)
        {
            if (estateIds.Count == 0 || adminIds.Count == 0)
            {
                return;
            }

            var existingAssignments = await db.Assignments
                .AsNoTracking()
                .Where(a => estateIds.Contains(a.FromId) && adminIds.Contains(a.ToId) && a.RoleId == RoleConstants.Rightholder)
                .ToDictionaryAsync(a => (a.FromId, a.ToId), cancellationToken);

            var existingAssignmentIds = existingAssignments.Values.Select(a => a.Id).ToList();
            var existingPackages = (await db.AssignmentPackages
                .AsNoTracking()
                .Where(p => existingAssignmentIds.Contains(p.AssignmentId) && EstateAdministratorPackages.Contains(p.PackageId))
                .Select(p => new { p.AssignmentId, p.PackageId })
                .ToListAsync(cancellationToken))
                .Select(p => (p.AssignmentId, p.PackageId))
                .ToHashSet();

            foreach (var estateId in estateIds)
            {
                foreach (var adminId in adminIds)
                {
                    if (!existingAssignments.TryGetValue((estateId, adminId), out var estateAssignment))
                    {
                        estateAssignment = new Assignment
                        {
                            FromId = estateId,
                            ToId = adminId,
                            RoleId = RoleConstants.Rightholder,
                        };

                        db.Assignments.Add(estateAssignment);
                    }

                    foreach (var packageId in EstateAdministratorPackages)
                    {
                        if (!existingPackages.Contains((estateAssignment.Id, packageId)))
                        {
                            db.AssignmentPackages.Add(new AssignmentPackage
                            {
                                AssignmentId = estateAssignment.Id,
                                PackageId = packageId,
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes the Rightholder assignment from every estate to every administrator, including its packages.
        /// Assignments and packages are loaded in two queries regardless of the number of estates.
        /// Changes are added to the context and must be saved by the caller.
        /// </summary>
        private async Task RevokeEstateAccess(List<Guid> estateIds, List<Guid> adminIds, CancellationToken cancellationToken)
        {
            if (estateIds.Count == 0 || adminIds.Count == 0)
            {
                return;
            }

            var estateAssignments = await db.Assignments
                .AsTracking()
                .Where(a => estateIds.Contains(a.FromId) && adminIds.Contains(a.ToId) && a.RoleId == RoleConstants.Rightholder)
                .ToListAsync(cancellationToken);

            if (estateAssignments.Count == 0)
            {
                return;
            }

            var estateAssignmentIds = estateAssignments.Select(a => a.Id).ToList();
            var estatePackages = await db.AssignmentPackages
                .AsTracking()
                .Where(p => estateAssignmentIds.Contains(p.AssignmentId))
                .ToListAsync(cancellationToken);
            db.AssignmentPackages.RemoveRange(estatePackages);

            // TODO: Open point in https://github.com/Altinn/altinn-auth/pull/4415: should we revoke the whole Rightholder
            // assignment or only the three estate administrator packages? For now the whole assignment is removed,
            // as it is better to remove it than to leave a user with access they should not have.
            db.Assignments.RemoveRange(estateAssignments);
        }

        private static readonly Guid[] EstateAdministratorPackages =
        [
            PackageConstants.MainAdministrator,
            PackageConstants.BankruptcyEstateReadAccess,
            PackageConstants.BankruptcyEstateWriteAccess,
        ];

        /// <inheritdoc />
        public async Task<ValidationProblemInstance?> RevokeBankruptcyEstateAdministrator(Guid partyUuid, Guid toUuid, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default)
        {
            var options = new ConnectionOptions(configureConnections);
            (Entity from, Entity to) = await GetFromAndToEntities(partyUuid, toUuid, cancellationToken);
            var problem = ValidateWriteOpInput(from, to, options);
            if (problem is { })
            {
                return problem;
            }

            var existingAssignment = await db.Assignments
                .AsTracking()
                .Where(p => p.FromId == partyUuid && p.ToId == toUuid && p.RoleId == RoleConstants.BankruptcyEstateAdministrator)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingAssignment is not null)
            {
                var estateIds = await GetBankruptcyEstateIds(partyUuid, cancellationToken);
                await RevokeEstateAccess(estateIds, [toUuid], cancellationToken);

                db.Assignments.Remove(existingAssignment);
            }

            await db.SaveChangesAsync(cancellationToken);
            return null;
        }
    }

    /// <summary>
    /// Service for managing bankruptcy estate delegations.
    /// </summary>
    public interface IBankruptcyEstateDelegationService
    {
        /// <summary>
        /// Checks if a given party has any bankruptcy estates.
        /// </summary>
        /// <param name="partyUuid">The unique identifier of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result indicating whether the party has any bankruptcy estates.</returns>
        Task<bool> HasBankruptcyEstates(Guid partyUuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of bankruptcy estates for a given party.
        /// </summary>
        /// <param name="party">The unique identifier of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing a list of bankruptcy estates.</returns>
        Task<Result<List<CompactEntityDto>>> GetBankruptcyEstates(Guid party, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a list of bankruptcy estate administrators for a given party.
        /// </summary>
        /// <param name="partyUuid">The unique identifier of the party.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing a list of bankruptcy estate administrators.</returns>
        Task<Result<List<CompactEntityDto>>> GetBankruptcyEstateAdministrators(Guid partyUuid, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a bankruptcy estate administrator to a given party.
        /// </summary>
        /// <param name="partyUuid">The unique identifier of the party.</param>
        /// <param name="toUuid">The unique identifier of the administrator to add.</param>
        /// <param name="configureConnections">An action to configure connection options.</param>   
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing a list of bankruptcy estate administrators.</returns>
        Task<Result<AssignmentDto>> AddBankruptcyEstateAdministrator(Guid partyUuid, Guid toUuid, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes a bankruptcy estate administrator from a given party.
        /// </summary>
        /// <param name="partyUuid">The unique identifier of the party.</param>
        /// <param name="toUuid">The unique identifier of the administrator to revoke.</param>
        /// <param name="configureConnections">An action to configure connection options.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A result containing the revoked assignment.</returns>
        Task<ValidationProblemInstance?> RevokeBankruptcyEstateAdministrator(Guid partyUuid, Guid toUuid, Action<ConnectionOptions> configureConnections, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gives every bankruptcy estate administrator of the estate administrator party the Rightholder role
        /// from the estate, with the estate administrator packages. Used by the ER import when an
        /// EstateAdministrator role is added.
        /// </summary>
        /// <param name="estateId">The unique identifier of the estate (konkursbo).</param>
        /// <param name="estateAdministratorPartyId">The unique identifier of the estate administrator party (bostyrer).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The number of changed rows.</returns>
        Task<int> GrantEstateAccessToAdministrators(Guid estateId, Guid estateAdministratorPartyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the Rightholder role from the estate for every bankruptcy estate administrator of the
        /// estate administrator party. Used by the ER import when an EstateAdministrator role is removed.
        /// </summary>
        /// <param name="estateId">The unique identifier of the estate (konkursbo).</param>
        /// <param name="estateAdministratorPartyId">The unique identifier of the estate administrator party (bostyrer).</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The number of changed rows.</returns>
        Task<int> RevokeEstateAccessFromAdministrators(Guid estateId, Guid estateAdministratorPartyId, CancellationToken cancellationToken = default);
    }
}
