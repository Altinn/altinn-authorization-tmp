//using Altinn.AccessMgmt.Core.Models;
//using Altinn.AccessMgmt.Persistence.Core.Contracts;
//using Altinn.AccessMgmt.Persistence.Core.Definitions;

//namespace Altinn.AccessMgmt.Repo.Definitions;

///// <inheritdoc/>
//public class GeneratedAssignmentPackageDefinition : BaseDbDefinition<GeneratedAssignmentPackage>, IDbDefinition
//{
//    /// <inheritdoc/>
//    public GeneratedAssignmentPackageDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
//    {
//    }

//    /// <inheritdoc/>
//    public void Define()
//    {
//        definitionRegistry.Define<GeneratedAssignmentPackage>(def =>
//        {
//            return;
//            def.IsView();

//            def.SetViewQuery(
//                """
//                with assignments as (
//                    select a.id, a.fromid, a.toid, a.roleid
//                    from dbo.assignment as a
//                    union all
//                    select a.id, a.fromid, a.toid, rm.getroleid
//                    from dbo.assignment as a
//                    inner join dbo.rolemap as rm on a.roleid = rm.hasroleid
//                )
//                select a.*, pck.packageid, 'Assignment' as type
//                from assignments as a
//                inner join dbo.assignmentpackage as pck on a.id = pck.assignmentid
//                union all
//                select a.*, pck.packageid, 'Role' as type
//                from assignments as a
//                inner join dbo.rolepackage as pck on a.roleid = pck.roleid;

//                union all

//                select

//                    fromAssignment.id,
//                    fromAssignment.fromid,
//                    toAssignment.fromid as viaid,
//                    toAssignment.toid,
//                    fromAssignment.roleid as fromroleid,
//                    toAssignment.roleid as toroleid,
//                    pck.packageid as packageid,
//                    'Delegated' as type

//                from dbo.delegation as d
//                inner join dbo.assignment as fromAssignment on d.fromid = fromAssignment.id
//                inner join dbo.assignment as toAssignment on d.toid = toAssignment.id
//                inner join dbo.delegationpackage as pck on d.id = pck.delegationid

//                """
//                );

//            def.RegisterProperty(t => t.Description);

//            ////def.RegisterExtendedProperty<ExtAreaGroup, EntityType>(t => t.EntityTypeId, t => t.Id, t => t.EntityType, cascadeDelete: true);

//        });
//    }
//}
