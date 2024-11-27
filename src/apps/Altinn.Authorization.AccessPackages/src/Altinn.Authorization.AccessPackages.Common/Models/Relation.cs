namespace Altinn.Authorization.AccessPackages.Models;

public class Relation // EntityRole
{
    public Guid Id { get; set; }
    public Guid FromId { get; set; }
    public Guid RoleId { get; set; }
    public bool IsDelegable { get; set; }
}
public class ExtRelation : Relation
{
    public Role Role { get; set; }
    public Entity From { get; set; }
    public List<RelationAssignment> Assignments { get; set; }
}
public class RelationAssignment
{
    public Guid Id { get; set; }
    public Guid RelationId { get; set; } // FromRelationId
    public Guid ToId { get; set; } // ToRelationId
}
public class ExtRelationAssignment : RelationAssignment
{
    public Relation Relation { get; set; } // FromRelation
    public Entity To { get; set; } // ToRelation
}



/*
SELECT 
RelationAssignment.Id AS Id,RelationAssignment.RelationId AS RelationId,RelationAssignment.ToId AS ToId
,_To.Id AS To_Id,_To.TypeId AS To_TypeId,_To.VariantId AS To_VariantId,_To.Name AS To_Name,_To.RefId AS To_RefId
,_Relation.Id AS Relation_Id,_Relation.EntityTypeId AS Relation_EntityTypeId,_Relation.ProviderId AS Relation_ProviderId,_Relation.Name AS Relation_Name,_Relation.Code AS Relation_Code,_Relation.Description AS Relation_Description,_Relation.Urn AS Relation_Urn
FROM dbo.RelationAssignment AS RelationAssignment
INNER JOIN dbo.Entity AS _To ON RelationAssignment.ToId = _To.Id 
INNER JOIN dbo.Role AS _Relation ON RelationAssignment.RelationId = _Relation.Id 
WHERE RelationAssignment.RelationId = @RelationId
 */
