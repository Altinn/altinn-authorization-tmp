using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class PolicyDefinition : BaseDbDefinition<Policy>, IDbDefinition
{
    /// <inheritdoc/>
    public PolicyDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<Policy>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtPolicy, Resource>(t => t.ResourceId, t => t.Id, t => t.Resource);

            def.RegisterUniqueConstraint([t => t.ResourceId, t => t.Name]);
        });
    }
}
