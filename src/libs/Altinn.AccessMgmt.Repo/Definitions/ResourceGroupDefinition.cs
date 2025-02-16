using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class ResourceGroupDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<ResourceGroup>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.ProviderId);

            def.RegisterExtendedProperty<ExtResourceGroup, Provider>(t => t.ProviderId, t => t.Id, t => t.Provider, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.ProviderId, t => t.Name]);
        });
    }
}
