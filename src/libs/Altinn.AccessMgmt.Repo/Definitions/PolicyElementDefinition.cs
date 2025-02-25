using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class PolicyElementDefinition : BaseDbDefinition<PolicyElement>, IDbDefinition
{
    /// <inheritdoc/>
    public PolicyElementDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<PolicyElement>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);
            def.RegisterProperty(t => t.PolicyId);
            def.RegisterProperty(t => t.ElementId);

            def.RegisterAsCrossReferenceExtended<ExtPolicyElement, Policy, Element>(
               defineA: (t => t.PolicyId, t => t.Id, t => t.Policy, CascadeDelete: true),
               defineB: (t => t.ElementId, t => t.Id, t => t.Element, CascadeDelete: true)
            );

            def.RegisterUniqueConstraint([t => t.PolicyId, t => t.ElementId]);
        });
    }
}
