using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ElementDefinition : BaseDbDefinition<Element>, IDbDefinition
{
    /// <inheritdoc/>
    public ElementDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<Element>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Urn);
            def.RegisterProperty(t => t.TypeId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtElement, ElementType>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtElement, Resource>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}
