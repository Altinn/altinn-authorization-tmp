using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for Element
/// </summary>
public class ElementDataService : ExtendedRepository<Element, ExtElement>, IElementService
{
    /// <summary>
    /// Data service for Element
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public ElementDataService(IDbExtendedRepo<Element, ExtElement> repo) : base(repo)
    //{
    //    Join<Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);
    //    Join<ElementType>(t => t.TypeId, t => t.Id, t => t.Type);
    //}
    public ElementDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
