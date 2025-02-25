using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationAssignmentResourceDefinition : BaseDbDefinition<DelegationAssignmentResource>, IDbDefinition
{
    /// <inheritdoc/>
    public DelegationAssignmentResourceDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<DelegationAssignmentResource>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.AssignmentResourceId);

            def.RegisterExtendedProperty<ExtDelegationAssignmentResource, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationAssignmentResource, AssignmentResource>(t => t.AssignmentResourceId, t => t.Id, t => t.AssignmentResource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.AssignmentResourceId]);
        });
    }
}
