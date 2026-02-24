using System.Diagnostics;
using Altinn.AccessMgmt.Core.Services.Contracts;
using Altinn.AccessMgmt.Core.Utils;
using Altinn.AccessMgmt.PersistenceEF.Constants;
using Altinn.AccessMgmt.PersistenceEF.Contexts;
using Altinn.AccessMgmt.PersistenceEF.Models;
using Altinn.AccessMgmt.PersistenceEF.Queries.Connection;
using Altinn.Authorization.Api.Contracts.AccessManagement;
using Altinn.Authorization.ProblemDetails;
using Microsoft.EntityFrameworkCore;

namespace Altinn.AccessMgmt.Core.Services
{
    public class ConnectionServiceServiceOwner(
        AppDbContext dbContext,
        ConnectionQuery connectionQuery
        ) : IConnectionServiceServiceOwner
    {

        /// <summary>
        /// Allows service owners to 
        /// </summary>
        public async Task<Result<AssignmentPackageDto>> AddPackage(Guid fromId, Guid toId, Guid packageId, Action<ConnectionOptions> configureConnection = null, CancellationToken cancellationToken = default)
        {
            ConnectionOptions options = new(configureConnection);
            (Entity from, Entity to) = await GetFromAndToEntities(fromId, toId, cancellationToken);

            // Look for existing direct rightholder assignment
            var assignment = await dbContext.Assignments
                .AsNoTracking()
                .Where(a => a.FromId == fromId)
                .Where(a => a.ToId == toId)
                .Where(a => a.RoleId == RoleConstants.Rightholder.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (assignment == null)
            {
                assignment = new Assignment()
                {
                    FromId = fromId,
                    ToId = toId,
                    RoleId = RoleConstants.Rightholder
                };

                await dbContext.Assignments.AddAsync(assignment, cancellationToken);
            }
            else
            {
                var existingAssignmentPackage = await dbContext.AssignmentPackages
                    .AsNoTracking()
                    .Where(a => a.AssignmentId == assignment.Id)
                    .Where(a => a.PackageId == packageId)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingAssignmentPackage is { })
                {
                    return DtoMapper.Convert(existingAssignmentPackage);
                }
            }

            var newAssignmentPackage = new AssignmentPackage()
            {
                AssignmentId = assignment.Id,
                PackageId = packageId,
            };

            await dbContext.AssignmentPackages.AddAsync(newAssignmentPackage, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return DtoMapper.Convert(newAssignmentPackage);
        }

        private async Task<(Entity From, Entity To)> GetFromAndToEntities(Guid? fromId, Guid? toId, CancellationToken cancellationToken)
        {
            if (fromId is null && toId is null)
            {
                throw new UnreachableException();
            }

            List<Entity> entities = await dbContext.Entities
                .AsNoTracking()
                .Where(e => e.Id == fromId || e.Id == toId)
                .Include(e => e.Type)
                .ToListAsync(cancellationToken);

            Entity fromEntity = entities.FirstOrDefault(e => e.Id == fromId);
            Entity toEntity = entities.FirstOrDefault(e => e.Id == toId);

            return (fromEntity, toEntity);
        }
    }
}
