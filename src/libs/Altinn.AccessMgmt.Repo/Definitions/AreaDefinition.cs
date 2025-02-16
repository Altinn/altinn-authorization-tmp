using Altinn.AccessMgmt.DbAccess.Contracts;
using Altinn.AccessMgmt.DbAccess.Helpers;
using Altinn.AccessMgmt.Models;

namespace Altinn.AccessMgmt.Repo.Definitions;

/// <inheritdoc/>
public class AreaDefinition : IDbDefinition
{
    /// <inheritdoc/>
    public void Define()
    {
        DefinitionStore.Define<Area>(def =>
        {
            def.EnableHistory();
            def.EnableTranslation();
            def.RegisterPrimaryKey([t => t.Id]);
            def.RegisterProperty(t => t.Id);

            def.RegisterProperty(t => t.Name);
            def.RegisterProperty(t => t.Description);
            def.RegisterProperty(t => t.IconName);
            def.RegisterProperty(t => t.GroupId);
            def.RegisterProperty(t => t.Urn);

            def.RegisterExtendedProperty<ExtArea, AreaGroup>(t => t.GroupId, t => t.Id, t => t.Group, cascadeDelete: true);
            def.RegisterUniqueConstraint([t => t.Name]);
        });
    }
}
