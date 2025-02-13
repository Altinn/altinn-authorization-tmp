using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Delegation

public class DelegationRoleResourceDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<DelegationRoleResource>(def =>
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

#endregion
