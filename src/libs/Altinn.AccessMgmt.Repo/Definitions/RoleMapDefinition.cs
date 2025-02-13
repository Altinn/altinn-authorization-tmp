using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Role

public class RoleMapDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<RoleMap>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.HasRoleId);
            def.RegisterProperty(t => t.GetRoleId);

            def.RegisterExtendedProperty<ExtRoleMap, Role>(t => t.HasRoleId, t => t.Id, t => t.HasRole, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtRoleMap, Role>(t => t.GetRoleId, t => t.Id, t => t.GetRole, cascadeDelete: false);

            def.RegisterUniqueConstraint([t => t.HasRoleId, t => t.GetRoleId]);
        });
    }
}

#endregion
