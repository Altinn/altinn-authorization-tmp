using Altinn.AccessManagement.Core.Errors;
using Altinn.AccessMgmt.Core.Appsettings;
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
    public class BankruptcyDelegationService(AppDbContext db, IOptions<CoreAppsettings> appsettings) : IBankruptcyDelegationService
    {
        private async Task<List<Assignment>> GetBankruptcyEstateAssignmentsForParty(Guid party, CancellationToken cancellationToken = default)
        {
            return await db.Assignments
                .AsNoTracking()
                .Where(a => a.ToId == party && a.RoleId == RoleConstants.EstateAdministrator)
                .Include(a => a.FromId)
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

        public Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateForParty(Guid party, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForUser(Guid party, Guid? user, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            List<Guid> estates = assignments.Select(a => a.FromId).ToList();

            var packages = (await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken)).Select(p => p.Id).ToList();

            var userAssignments = await db.Assignments
                .AsNoTracking()
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.ToId == user && a.Assignment.RoleId == RoleConstants.Rightholder && packages.Contains(a.AssignmentPackage.PackageId))
                .Include(a => a.Assignment.From)
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

        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForEstate(Guid party, Guid estate, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            List<Guid> estates = assignments.Select(a => a.FromId).ToList();

            var packages = (await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken)).Select(p => p.Id).ToList();

            var estateAssignments = await db.Assignments
                .AsNoTracking()
                .Join(db.AssignmentPackages, a => a.Id, ap => ap.AssignmentId, (a, ap) => new { Assignment = a, AssignmentPackage = ap })
                .Where(a => a.Assignment.FromId == estate && a.Assignment.RoleId == RoleConstants.Rightholder && packages.Contains(a.AssignmentPackage.PackageId))
                .Include(a => a.Assignment.From)
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

            return DtoMapper.Convert(assignmentPackages);
        }

        public Task<Result<List<BankruptcyEstateAssignmentsDto>>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packages, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

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
    }

    /// <summary>
    /// Service for managing client delegations and delegation of access packages.
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
        /// Gets the clients for a given party, filtered by roles and packages.
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
        /// Adds packages to a user for a specific bankruptcy estate.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="packages">The list of packages to add.</param>
        /// <param name="configureConnections">Optional action to configure connection options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of updated assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> AddBankruptcyEstatePackagesToUser(Guid party, Guid user, Guid estate, List<string> packages, Action<ConnectionOptions> configureConnections = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes packages from a user for a specific bankruptcy estate.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="packages">The list of packages to revoke.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of updated assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packages, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if a given estate is connected to the party
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="estate">The estate identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the estate is connected to the party, or a problem detail if an error occurs.</returns>
        Task<bool> CheckBankruptcyEstateConnection(Guid party, Guid estate, CancellationToken cancellationToken = default);
    }
}
