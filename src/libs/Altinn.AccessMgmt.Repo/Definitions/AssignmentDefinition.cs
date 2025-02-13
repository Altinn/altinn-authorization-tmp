using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Assignment

public class AssignmentDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<Assignment>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.FromId);
            def.RegisterProperty(t => t.ToId);
            def.RegisterProperty(t => t.RoleId);
            def.RegisterProperty(t => t.IsDelegable);

            def.RegisterExtendedProperty<ExtAssignment, Entity>(t => t.FromId, t => t.Id, t => t.From, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtAssignment, Entity>(t => t.ToId, t => t.Id, t => t.To, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtAssignment, Role>(t => t.RoleId, t => t.Id, t => t.Role, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.FromId, t => t.ToId, t => t.RoleId]);
        });
    }
}

#endregion
