using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for GroupDelegation
/// </summary>
public class GroupDelegationDataService : ExtendedRepository<GroupDelegation, ExtGroupDelegation>, IGroupDelegationService
{
    /// <summary>
    /// Data service for GroupDelegation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public GroupDelegationDataService(IDbExtendedRepo<GroupDelegation, ExtGroupDelegation> repo) : base(repo)
    //{
    //    Join<Assignment>(t => t.FromId, t => t.Id, t => t.From);
    //    Join<EntityGroup>(t => t.ToId, t => t.Id, t => t.To);
    //    Join<Assignment>(t => t.SourceId, t => t.Id, t => t.Source);
    //    Join<Assignment>(t => t.ViaId, t => t.Id, t => t.Via);
    //}
    public GroupDelegationDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
