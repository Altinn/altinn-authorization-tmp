using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Delegation
/// </summary>
public class DelegationDataService : ExtendedRepository<Delegation, ExtDelegation>, IDelegationService
{
    /// <summary>
    /// Data service for Delegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public DelegationDataService(IDbExtendedRepo<Delegation, ExtDelegation> repo) : base(repo)
    //{
    //    Join<Assignment>(t => t.FromId, t => t.Id, t => t.From);
    //    Join<Assignment>(t => t.ToId, t => t.Id, t => t.To);
    //    Join<Assignment>(t => t.SourceId, t => t.Id, t => t.Source);
    //    Join<Assignment>(t => t.ViaId, t => t.Id, t => t.Via);
    //}
    public DelegationDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
