using Altinn.AccessMgmt.Repo.Data.Contracts;
using Altinn.AccessMgmt.Models;
using Altinn.AccessMgmt.DbAccess.Services;
using Microsoft.Extensions.Options;
using Altinn.AccessMgmt.DbAccess.Models;
using Npgsql;
using Altinn.AccessMgmt.DbAccess.Contracts;

namespace Altinn.AccessMgmt.Repo.Data.Services;

/// <summary>
/// Data service for EntityVariantRole
/// </summary>
public class EntityVariantRoleDataService : CrossRepository<EntityVariantRole, ExtEntityVariantRole, EntityVariant, Role>, IEntityVariantRoleService
{
    /// <summary>
    /// Data service for EntityVariantRole
    /// </summary>
    /// <param name="repo">Extended repo</param>
    //public EntityVariantRoleDataService(IDbCrossRepo<EntityVariant, EntityVariantRole, Role> repo) : base(repo)
    //{
    //    CrossRepo.SetCrossColumns("variantid", "roleid");
    //}
    public EntityVariantRoleDataService(IOptions<DbAccessConfig> options, NpgsqlDataSource connection, IDbConverter dbConverter) : base(options, connection, dbConverter)
    {
    }
}
