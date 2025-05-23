using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class NewConnectionDefinition : BaseDbDefinition<ConnectionV2>, IDbDefinition
{
    /// <inheritdoc/>
    public NewConnectionDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<ConnectionV2>(def =>
        {
            def.SetVersion(1);
            def.SetType(DbDefinitionType.View);

            def.RegisterExtProperty<ExtConnectionV2>(t => t.FromId, t => t.From, "CompactEntity");
            def.RegisterExtProperty<ExtConnectionV2>(t => t.RoleId, t => t.Role, "CompactRole");
            def.RegisterExtProperty<ExtConnectionV2>(t => t.ViaId, t => t.Via, "CompactEntity", nullable: true);
            def.RegisterExtProperty<ExtConnectionV2>(t => t.ViaRoleId, t => t.ViaRole, "CompactRole", nullable: true);
            def.RegisterExtProperty<ExtConnectionV2>(t => t.ToId, t => t.To, "CompactEntity");
            def.RegisterExtProperty<ExtConnectionV2>(t => t.PackageId, t => t.Package, "CompactPackage", nullable: true);
            def.RegisterProperty(t => t.Reason);

            def.SetQuery(BasicScript());

            def.AddManualDependency<Entity>();
            def.AddManualDependency<Role>();
            def.AddManualDependency<RoleMap>();
            def.AddManualDependency<Assignment>();
            def.AddManualDependency<Delegation>();
            def.AddManualDependency<AssignmentPackage>();
            def.AddManualDependency<DelegationPackage>();
            def.AddManualDependency<Package>();
        });
    }

    private string BasicScript()
    {
        return $"""
        SELECT a.fromid,
        a.roleid,
        NULL::uuid     AS viaid,
        NULL::uuid     AS viaroleid,
        a.toid,
        ap.packageid,
        'Direct'::text AS reason
        FROM dbo.assignment a
        LEFT OUTER JOIN dbo.assignmentpackage ap ON ap.assignmentid = a.id
        UNION ALL
        SELECT a.fromid,
        a.roleid,
        a.toid          AS viaid,
        a2.roleid       AS viaroleid,
        a2.toid,
        ap.packageid,
        'KeyRole'::text AS reason
        FROM dbo.assignment a
        JOIN dbo.assignment a2 ON a.toid = a2.fromid
        JOIN dbo.role r ON a2.roleid = r.id AND r.iskeyrole = true
        LEFT OUTER JOIN dbo.assignmentpackage ap ON ap.assignmentid = a.id
        UNION ALL
        SELECT fa.fromid,
        fa.roleid,
        fa.toid            AS viaid,
        ta.roleid          AS viaroleid,
        ta.toid,
        dp.packageid,
        'Delegation'::text AS reason
        FROM dbo.delegation d
        JOIN dbo.assignment fa ON fa.id = d.fromid
        JOIN dbo.assignment ta ON ta.id = d.toid
        LEFT OUTER JOIN dbo.delegationpackage dp ON dp.delegationid = d.id;
        """;
    }
}
