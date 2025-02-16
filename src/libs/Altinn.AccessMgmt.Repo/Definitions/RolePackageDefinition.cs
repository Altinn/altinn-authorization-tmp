using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class RolePackageDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<RolePackage>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.RoleId);
            def.RegisterProperty(t => t.PackageId);
            def.RegisterProperty(t => t.HasAccess);
            def.RegisterProperty(t => t.CanDelegate);
            def.RegisterProperty(t => t.EntityVariantId, nullable: true);

            def.RegisterExtendedProperty<ExtRolePackage, Role>(t => t.RoleId, t => t.Id, t => t.Role, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtRolePackage, Package>(t => t.PackageId, t => t.Id, t => t.Package, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtRolePackage, EntityVariant>(t => t.EntityVariantId, t => t.Id, t => t.EntityVariant, cascadeDelete: false, optional: true);

            def.RegisterUniqueConstraint([t => t.RoleId, t => t.PackageId, t => t.EntityVariantId]);
        });
    }
}
