using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Delegation

public class DelegationAssignmentPackageDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<DelegationAssignmentPackage>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.DelegationId);
            def.RegisterProperty(t => t.AssignmentPackageId);

            def.RegisterExtendedProperty<ExtDelegationAssignmentPackage, Delegation>(t => t.DelegationId, t => t.Id, t => t.Delegation, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtDelegationAssignmentPackage, AssignmentPackage>(t => t.AssignmentPackageId, t => t.Id, t => t.AssignmentPackage, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.DelegationId, t => t.AssignmentPackageId]);
        });
    }
}

#endregion
