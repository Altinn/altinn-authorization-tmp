using Altinn.AccessMgmt.Core.Appsettings;
using Altinn.AccessMgmt.Core.Utils;
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

        private async Task<List<Guid>> GetBankruptcyEstateAssignmentPackagesForParty(CancellationToken cancellationToken = default)
        {
            return await db.RolePackages
                .AsNoTracking()
                .Where(rp => rp.RoleId == RoleConstants.EstateAdministrator)
                .Select(rp => rp.PackageId)
                .ToListAsync(cancellationToken);
        }

        public Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateForParty(Guid party, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateAssignmentsForUser(Guid party, Guid? user, CancellationToken cancellationToken = default)
        {
            var assignments = await GetBankruptcyEstateAssignmentsForParty(party, cancellationToken);

            List<Guid> estates = assignments.Select(a => a.FromId).ToList();

            var packages = await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken);

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

            var packages = await GetBankruptcyEstateAssignmentPackagesForParty(cancellationToken);

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
        public Task<Result<List<BankruptcyEstateAssignmentsDto>>> AddBankruptcyEstatePackagesToUser(Guid party, Guid user, Guid estate, List<string> packages)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<BankruptcyEstateAssignmentsDto>>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packages)
        {
            throw new NotImplementedException();
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
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> GetBankruptcyEstateForParty(Guid party, CancellationToken cancellationToken = default)

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
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of updated assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> AddBankruptcyEstatePackagesToUser(Guid party, Guid user, Guid estate, List<string> packages);

        /// <summary>
        /// Revokes packages from a user for a specific bankruptcy estate.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="user">The user identifier.</param>
        /// <param name="estate">The bankruptcy estate identifier.</param>
        /// <param name="packages">The list of packages to revoke.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of updated assignments or a problem detail if an error occurs.</returns>
        Task<Result<List<BankruptcyEstateAssignmentsDto>>> RevokeBankruptcyEstatePackagesFromUser(Guid party, Guid user, Guid estate, List<string> packages);        
    }
}
