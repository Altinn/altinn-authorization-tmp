using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class RoleResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<RoleResource>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.RoleId);
            def.RegisterProperty(t => t.ResourceId);

            def.RegisterExtendedProperty<ExtRoleResource, Role>(t => t.RoleId, t => t.Id, t => t.Role, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtRoleResource, Resource>(t => t.ResourceId, t => t.Id, t => t.Resource, cascadeDelete: false);

            def.RegisterUniqueConstraint([t => t.RoleId, t => t.ResourceId]);
        });
    }
}
