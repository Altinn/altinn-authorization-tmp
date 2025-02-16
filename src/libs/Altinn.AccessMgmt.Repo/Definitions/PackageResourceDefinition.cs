using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class PackageResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<PackageResource>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.PackageId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtPackageResource, Package>(t => t.PackageId, t => t.Id, t => t.Package, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtPackageResource, Resource>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.PackageId, t => t.ResourceId]);
        });
    }
}
