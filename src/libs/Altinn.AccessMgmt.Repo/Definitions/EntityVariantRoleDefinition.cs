using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Role

public class EntityVariantRoleDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<EntityVariantRole>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.VariantId);
            def.RegisterProperty(t => t.RoleId);

            def.RegisterExtendedProperty<ExtEntityVariantRole, EntityVariant>(t => t.VariantId, t => t.Id, t => t.Variant, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtEntityVariantRole, Role>(t => t.RoleId, t => t.Id, t => t.Role, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.VariantId, t => t.RoleId]);
        });
    }
}

#endregion
