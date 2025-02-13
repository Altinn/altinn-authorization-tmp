using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Assignment

public class AssignmentPackageDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<AssignmentPackage>(def =>
        {
            def.EnableHistory();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.AssignmentId);
            def.RegisterProperty(t => t.PackageId);

            def.RegisterExtendedProperty<ExtAssignmentPackage, Entity>(t => t.AssignmentId, t => t.Id, t => t.Assignment, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtAssignmentPackage, Entity>(t => t.PackageId, t => t.Id, t => t.Package, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.AssignmentId, t => t.PackageId]);
        });
    }
}

#endregion
