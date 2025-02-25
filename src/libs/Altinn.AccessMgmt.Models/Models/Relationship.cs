using System.Data;

namespace Altinn.AccessMgmt.Core.Models;

/// <summary>
/// Experimental
/// VIEW
/// </summary>
public class Relationship
{
    /// <summary>
    /// Identity
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// From
    /// </summary>
    public Guid FromId { get; set; }
    
    /// <summary>
    /// Role
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// To
    /// </summary>
    public Guid ToId { get; set; }
    
    /// <summary>
    /// Via
    /// </summary>
    public Guid ViaId { get; set; }

    /// <summary>
    /// Type
    /// </summary>
    public string Type { get; set; }
}

/*
 QUERY : 


WITH T1 as (

--DIRECT
    select ass.toid as src, r.name as role, e.name as target, '' as reson, 'direct' as type
    from dbo.assignment as ass
             inner join dbo.entity as e on ass.fromid = e.id
             inner join dbo.role as r on ass.roleid = r.id
--where ass.toid = '40ddd152-e685-437f-9efb-3e89e6854206'

    union all

--GROUP
    select mem.memberid as src, r.name, e.name, grp.name, 'group' as type
    from dbo.groupmember as mem
             inner join dbo.entitygroup as grp ON mem.groupid = grp.id
             inner join dbo.delegationgroup as delGrp on grp.id = delGrp.groupid
             inner join dbo.delegation as del on delGrp.delegationid = del.id
             inner join dbo.assignment as ass on del.assignmentid = ass.id
             inner join dbo.entity as e on ass.fromid = e.id
             inner join dbo.role as r on ass.roleid = r.id
--where memberid = '40ddd152-e685-437f-9efb-3e89e6854206'

    union all

--DELEGATED
    select baseAss.toid as src, r.name, e.name, baseEntity.name, 'delegation' as type
    from dbo.assignment as baseAss
        inner join dbo.entity as baseEntity on baseAss.fromid = baseEntity.id
             inner join dbo.delegationassignment as delAss on baseAss.id = delAss.assignmentid
             inner join dbo.delegation as del on delAss.delegationid = del.id
             inner join dbo.assignment as ass on del.assignmentid = ass.id
             inner join dbo.entity as e on ass.fromid = e.id
             inner join dbo.role as r on ass.roleid = r.id
--where baseAss.toid = '40ddd152-e685-437f-9efb-3e89e6854206';
)
select e.name as src, role, target, reson, type
from T1
inner join dbo.entity as e on T1.src = e.id
order by e.name, target, role
;



 */
