using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class EntityDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<Entity>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.RefId);
            def.RegisterProperty(t => t.TypeId);
            def.RegisterProperty(t => t.VariantId);

            def.RegisterExtendedProperty<ExtEntity, EntityType>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtEntity, EntityVariant>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: false);
            def.RegisterUniqueConstraint([t => t.Name, t => t.TypeId, t => t.RefId]);
        });
    }
}
