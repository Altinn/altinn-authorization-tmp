using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationRolePackageResourceDefinition : BaseDbDefinition<DelegationRolePackageResource>, IDbDefinition
{
    /// <inheritdoc/>
    public DelegationRolePackageResourceDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<DelegationRolePackageResource>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.RolePackageId);
            def.RegisterProperty(t => t.PackageResourceId);

            def.RegisterExtendedProperty<ExtDelegationRolePackageResource, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationRolePackageResource, AssignmentPackage>(t => t.RolePackageId, t => t.Id, t => t.RolePackage, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtDelegationRolePackageResource, PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.RolePackageId, t => t.PackageResourceId]);
        });
    }
}
