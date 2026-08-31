using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Appsettings;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.Core.Validation;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altinn.AccessMgmt.Core.Services
{
    public class BankruptcyDelegationService(AppDbContext db, IConnectionService ConnectionService) : IBankruptcyDelegationService
    {
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
        public async Task<Result<bool>> AddCreditor(Guid party, Guid creditor, Guid estate, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default)
        {
            var result = await ConnectionService.AddRightholder(estate, creditor, configureConnections, cancellationToken);
            if (result.IsProblem)
            {
                return result.Problem;
            }

            var assignmentId = result.Value.Id;

            var existingAssignmentPackage = await db.AssignmentPackages
                    .AsNoTracking()
                    .Where(ap => ap.AssignmentId == assignmentId && ap.PackageId == PackageConstants.BankruptcyEstateReadAccess.Entity.Id)
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
    }

    /// <summary>
    /// Service for managing client delegations and delegation of access packageCodes.
    /// </summary>
    public interface IBankruptcyDelegationService
    {
        /// <summary>
        /// Gets the bankruptcy estate assignments for a given party.
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
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The estate identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of bankruptcy estate assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForEstate(Guid party, Guid estate, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds packageCodes to a user for a specific bankruptcy estate.
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
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="creditor">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. true if read access is added and false if it alredy exists</returns>
        Task<Result<bool>> AddCreditor(Guid party, Guid creditor, Guid estate, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the package BankruptcyEstateReadAccess from the assignment between the creditor and the bankruptcy estate if the rettighetshaver assignment holds no more content the assignment is also removed..
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="creditor">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. true if read access is revoked and false if it alredy was revoked</returns>
        Task<Result<bool>> RevokeCreditor(Guid party, Guid creditor, Guid estate, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the list of creditors for a specific bankruptcy estate.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A problem details if some error occurs. List of creditors if successful.</returns>
        Task<Result<bool>> GetCreditors(Guid party, Guid estate, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);
    }
}
