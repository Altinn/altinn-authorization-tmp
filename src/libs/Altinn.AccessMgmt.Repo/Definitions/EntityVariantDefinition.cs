using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#endregion
#region Entity
public class EntityVariantDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<EntityVariant>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.TypeId);

            def.RegisterExtendedProperty<ExtEntityVariant, EntityType>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.TypeId, t => t.Name]);
        });
    }
}

#endregion
