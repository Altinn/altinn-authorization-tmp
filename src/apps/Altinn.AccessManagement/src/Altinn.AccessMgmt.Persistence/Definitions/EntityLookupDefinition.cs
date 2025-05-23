﻿using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class EntityLookupDefinition : BaseDbDefinition<EntityLookup>, IDbDefinition
{
    /// <inheritdoc/>
    public EntityLookupDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<EntityLookup>(def =>
        {
            def.EnableAudit();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.EntityId);
            def.RegisterProperty(t => t.Key);
            def.RegisterProperty(t => t.Value);

            def.RegisterExtendedProperty<ExtEntityLookup, Entity>(t => t.EntityId, t => t.Id, t => t.Entity, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.EntityId, t => t.Key], includedProperties: [t => t.Value, t => t.Id]);
        });
    }
}
