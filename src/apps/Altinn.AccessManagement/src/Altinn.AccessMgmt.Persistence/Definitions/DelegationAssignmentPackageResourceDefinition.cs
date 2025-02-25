using Altinn.AccessMgmt.Core.Models;
using Altinn.AccessMgmt.Persistence.Core.Contracts;
using Altinn.AccessMgmt.Persistence.Core.Definitions;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class DelegationAssignmentPackageResourceDefinition : BaseDbDefinition<DelegationAssignmentPackageResource>, IDbDefinition
{
    /// <inheritdoc/>
    public DelegationAssignmentPackageResourceDefinition(DbDefinitionRegistry definitionRegistry) : base(definitionRegistry)
    {
    }

    /// <inheritdoc/>
    public void Define()
    {
        definitionRegistry.Define<DelegationAssignmentPackageResource>(def =>
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
