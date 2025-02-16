using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ResourceDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<Resource>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.RefId);
            def.RegisterProperty(t => t.ProviderId);
            def.RegisterProperty(t => t.TypeId);
            def.RegisterProperty(t => t.GroupId);

            def.RegisterExtendedProperty<ExtResource, Provider>(t => t.ProviderId, t => t.Id, t => t.Provider, cascadeDelete: true);
            def.RegisterExtendedProperty<ExtResource, ResourceType>(t => t.TypeId, t => t.Id, t => t.Type, cascadeDelete: false);
            def.RegisterExtendedProperty<ExtResource, ResourceGroup>(t => t.GroupId, t => t.Id, t => t.Group, cascadeDelete: true);

            def.RegisterUniqueConstraint([t => t.ProviderId, t => t.GroupId, t => t.RefId]);
        });
    }
}
