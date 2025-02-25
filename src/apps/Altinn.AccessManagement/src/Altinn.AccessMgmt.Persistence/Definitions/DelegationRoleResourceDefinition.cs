using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationRoleResourceDefinition : BaseDbDefinition<DelegationRoleResource>, IDbDefinition
{
    /// <inheritdoc/>
    public DelegationRoleResourceDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<DelegationRoleResource>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.RoleResourceId);

            def.RegisterExtendedProperty<ExtDelegationRoleResource, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationRoleResource, RoleResource>(t => t.RoleResourceId, t => t.Id, t => t.RoleResource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.RoleResourceId]);
        });
    }
}
