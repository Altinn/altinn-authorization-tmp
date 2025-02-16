using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class EntityGroupDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<EntityGroup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.OwnerId);
            def.RegisterProperty(t => t.RequireRole);

            def.RegisterExtendedProperty<ExtEntityGroup, Entity>(t => t.OwnerId, t => t.Id, t => t.Owner, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.OwnerId, t => t.Name]);
        });
    }
}
