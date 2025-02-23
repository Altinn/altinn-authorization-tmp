using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo.Contracts;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Services;

/// <summary>
/// Data service for DelegationAssignmentPackage
/// </summary>
public class DelegationAssignmentPackageDataService : ExtendedRepository<DelegationAssignmentPackage, ExtDelegationAssignmentPackage>, IDelegationAssignmentPackageService
{
    /// <inheritdoc/>
    public DelegationAssignmentPackageDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
