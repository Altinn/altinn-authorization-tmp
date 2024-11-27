using Altinn.Authorization.AccessPackages.DbAccess.Data.Contracts;
using Altinn.Authorization.AccessPackages.DbAccess.Data.Services;
using Altinn.Authorization.AccessPackages.Models;
using Altinn.Authorization.AccessPackages.Repo.Data.Contracts;

namespace Altinn.Authorization.AccessPackages.Repo.Data.Services;

//// TODO: IVAR

/// <summary>
/// Data service for Relation
/// </summary>
public class RelationDataService : BaseExtendedDataService<Relation, ExtRelation>, IRelationService
{
    /// <summary>
    /// Data service for Relation
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RelationDataService(IDbExtendedRepo<Relation, ExtRelation> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.FromId, t => t.Id, t => t.From);
        ExtendedRepo.Join<Role>(t => t.RoleId, t => t.Id, t => t.Role);
        ExtendedRepo.Join<RelationAssignment>(t => t.Id, t => t.RelationId, t => t.Assignments, isList: true);
    }
}

/*
SELECT 
Relation.Id AS Id,Relation.FromId AS FromId,Relation.RoleId AS RoleId,Relation.IsDelegable AS IsDelegable
,_From.Id AS From_Id,_From.TypeId AS From_TypeId,_From.VariantId AS From_VariantId,_From.Name AS From_Name,_From.RefId AS From_RefId
,_Role.Id AS Role_Id,_Role.EntityTypeId AS Role_EntityTypeId,_Role.ProviderId AS Role_ProviderId,_Role.Name AS Role_Name,_Role.Code AS Role_Code,_Role.Description AS Role_Description,_Role.Urn AS Role_Urn
,COALESCE((SELECT JSON_AGG(ROW_TO_JSON(RelationAssignment)) FROM dbo.RelationAssignment AS RelationAssignment WHERE RelationAssignment.RelationId = Relation.Id), '[]') AS Assignments
FROM dbo.Relation AS Relation
INNER JOIN dbo.Entity AS _From ON Relation.FromId = _From.Id 
INNER JOIN dbo.Role AS _Role ON Relation.RoleId = _Role.Id 
WHERE Relation.FromId = @FromId
 */

/// <summary>
/// Data service for RelationAssignment
/// </summary>
public class RelationAssignmentDataService : BaseExtendedDataService<RelationAssignment, ExtRelationAssignment>, IRelationAssignmentService
{
    /// <summary>
    /// Data service for RelationAssignment
    /// </summary>
    /// <param name="repo">Extended repo</param>
    public RelationAssignmentDataService(IDbExtendedRepo<RelationAssignment, ExtRelationAssignment> repo) : base(repo)
    {
        ExtendedRepo.Join<Entity>(t => t.ToId, t => t.Id, t => t.To);
        ExtendedRepo.Join<Relation>(t => t.RelationId, t => t.Id, t => t.Relation);
    }
}
