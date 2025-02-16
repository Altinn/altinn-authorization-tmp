using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class EntityLookupDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<EntityLookup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.EntityId);
            def.RegisterProperty(t => t.Key);
            def.RegisterProperty(t => t.Value);

            def.RegisterExtendedProperty<ExtEntityLookup, Entity>(t => t.EntityId, t => t.Id, t => t.Entity, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.EntityId, t => t.Key]);
        });
    }
}
