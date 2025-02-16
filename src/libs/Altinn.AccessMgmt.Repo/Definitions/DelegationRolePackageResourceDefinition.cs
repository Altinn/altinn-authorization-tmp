using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationRolePackageResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<DelegationRolePackageResource>(def =>
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
