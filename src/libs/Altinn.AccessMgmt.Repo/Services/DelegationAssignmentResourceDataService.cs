using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.Repo.Contracts;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Altinn.AccessMgmt.Repo.Services;

/// <summary>
/// Data service for DelegationAssignmentResource
/// </summary>
public class DelegationAssignmentResourceDataService : ExtendedRepository<DelegationAssignmentResource, ExtDelegationAssignmentResource>, IDelegationAssignmentResourceService
{
    /// <inheritdoc/>
    public DelegationAssignmentResourceDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
