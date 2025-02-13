using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Delegation

public class DelegationRolePackageDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<DelegationRolePackage>(def =>
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

#endregion
