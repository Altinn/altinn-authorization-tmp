using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for ElementType
/// </summary>
public class ElementTypeDataService : BasicRepository<ElementType>, IElementTypeService
{
    /// <summary>
    /// Data service for ElementType
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public ElementTypeDataService(IDbBasicRepo<ElementType> repo) : base(repo) { }
    public ElementTypeDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
