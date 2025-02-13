using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;
using System.Text.RegularExpressions;

namespace Altinn.AccessMgmt.Repo.Definitions;
#region Package

public class PackageTagDefinition : IDbDefinition
{
    public void Define()
    {
        DefinitionStore.Define<PackageTag>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.PackageId);
            def.RegisterProperty(t => t.TagId);

            def.RegisterExtendedProperty<ExtPackageTag, Package>(t => t.PackageId, t => t.Id, t => t.Package, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtPackageTag, Tag>(t => t.TagId, t => t.Id, t => t.Tag, cascadeDelete: false);

            def.RegisterUniqueConstraint([t => t.PackageId, t => t.TagId]);
        });
    }
}

#endregion
