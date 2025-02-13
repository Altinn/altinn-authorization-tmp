using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Delegation

public class DelegationAssignmentResourceDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<DelegationAssignmentResource>(def =>
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

#endregion
