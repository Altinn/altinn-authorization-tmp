using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ResourceGroupDefinition : BaseDbDefinition<ResourceGroup>, IDbDefinition
{
    /// <inheritdoc/>
    public ResourceGroupDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<ResourceGroup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.ProviderId);

            def.RegisterExtendedProperty<ExtResourceGroup, Provider>(t => t.ProviderId, t => t.Id, t => t.Provider, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.ProviderId, t => t.Name]);
        });
    }
}
