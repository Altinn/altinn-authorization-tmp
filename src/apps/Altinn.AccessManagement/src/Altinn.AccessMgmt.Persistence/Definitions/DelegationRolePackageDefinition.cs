using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationRolePackageDefinition : BaseDbDefinition<DelegationRolePackage>, IDbDefinition
{
    /// <inheritdoc/>
    public DelegationRolePackageDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<DelegationRolePackage>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.RolePackageId);

            def.RegisterExtendedProperty<ExtDelegationRolePackage, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationRolePackage, RolePackage>(t => t.RolePackageId, t => t.Id, t => t.RolePackage, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.RolePackageId]);
        });
    }
}
