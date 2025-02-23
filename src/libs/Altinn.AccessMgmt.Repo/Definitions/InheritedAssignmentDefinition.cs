using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class InheritedAssignmentDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<InheritedAssignment>(def =>
        {
            def.IsView();
            //// def.EnableHistory();
            //// def.EnableTranslation();
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.FromId);
            def.RegisterProperty(t => t.ToId);
            def.RegisterProperty(t => t.RoleId);
            def.RegisterProperty(t => t.ViaRoleId!);
            def.RegisterProperty(t => t.Type);

            def.RegisterExtendedProperty<ExtInheritedAssignment, Entity>(t => t.FromId, t => t.Id, t => t.From);
            def.RegisterExtendedProperty<ExtInheritedAssignment, Entity>(t => t.ToId, t => t.Id, t => t.To);
            def.RegisterExtendedProperty<ExtInheritedAssignment, Role>(t => t.RoleId, t => t.Id, t => t.Role);
            def.RegisterExtendedProperty<ExtInheritedAssignment, Role>(t => t.ViaRoleId!, t => t.Id, t => t.ViaRole!, optional: true);

            def.AddViewDependency<RoleMap>();

            def.SetViewQuery($"""
            select a.id, a.fromid, a.toid, a.roleid, null as viaroleid, 'Direct' as type
            from dbo.assignment as a
            union all
            select a.id, a.fromid, a.toid, rm.getroleid as roleid, a.roleid as viaroleid, 'Inherited' as type
            from dbo.assignment as a
            inner join dbo.rolemap as rm on a.roleid = rm.hasroleid
            """);
        });
    }
}
