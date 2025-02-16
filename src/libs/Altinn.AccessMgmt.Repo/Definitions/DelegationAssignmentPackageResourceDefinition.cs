using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationAssignmentPackageResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<DelegationAssignmentPackageResource>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.AssignmentPackageId);
            def.RegisterProperty(t => t.PackageResourceId);

            def.RegisterExtendedProperty<ExtDelegationAssignmentPackageResource, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationAssignmentPackageResource, AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.AssignmentPackage, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtDelegationAssignmentPackageResource, PackageResource>(t => t.PackageResourceId, t => t.Id, t => t.PackageResource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.AssignmentPackageId, t => t.PackageResourceId]);
        });
    }
}
